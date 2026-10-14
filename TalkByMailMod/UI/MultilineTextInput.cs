using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewUI.Events;
using StardewUI.Graphics;
using StardewUI.Input;
using StardewUI.Layout;
using StardewUI;
using StardewValley;

namespace TalkByMailMod.UI;

/// <summary>
/// A plain, background-less multi-line text box for composing longer messages (e.g. letters).
/// Intended to be wrapped in your own <c>&lt;frame&gt;</c> if you want a border/background.
/// </summary>
/// <remarks>
/// This is a hand-rolled equivalent of StardewUI's built-in <c>TextInput</c>, which only supports a single line.
/// Word-wrapping and the caret/keyboard-capture logic are both implemented directly in this class rather than
/// reusing TextInput, because TextInput's internals are all private and non-virtual.
/// </remarks>
public class MultilineTextInput : StardewUI.View
{
    /// <summary>Raised whenever <see cref="Text"/> changes.</summary>
    public event EventHandler<EventArgs>? TextChanged;

    /// <summary>The full text currently entered, including any line breaks.</summary>
    public string Text
    {
        get => text;
        set
        {
            var newValue = value ?? "";
            if (newValue == text)
            {
                return;
            }
            text = newValue;
            caretPosition = text.Length;
            contentDirty = true;
            OnPropertyChanged(nameof(Text));
            TextChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>The zero-based character index of the caret within <see cref="Text"/>.</summary>
    public int CaretPosition
    {
        get => caretPosition;
        set
        {
            var clamped = Math.Clamp(value, 0, text.Length);
            if (clamped == caretPosition)
            {
                return;
            }
            caretPosition = clamped;
            ResetCaretBlink();
            OnPropertyChanged(nameof(CaretPosition));
        }
    }

    /// <summary>The font used to render and measure text. Defaults to <see cref="Game1.smallFont"/>.</summary>
    public SpriteFont Font
    {
        get => font;
        set
        {
            if (value == font)
            {
                return;
            }
            font = value;
            contentDirty = true;
            OnPropertyChanged(nameof(Font));
        }
    }

    /// <summary>Color used to draw the text and the caret.</summary>
    public Color TextColor { get; set; } = Game1.textColor;

    /// <summary>Maximum number of characters allowed, or 0 for unlimited.</summary>
    public int MaxLength { get; set; }

    private const double CaretBlinkIntervalSeconds = 0.53;

    private readonly TextInputSubscriber subscriber;
    private readonly List<LineInfo> lines = new();

    private string text = "";
    private int caretPosition;
    private SpriteFont font = Game1.smallFont;
    private bool contentDirty = true;
    private bool isCapturingInput;
    private bool caretVisible;
    private double caretBlinkTimer;

    public MultilineTextInput()
    {
        Focusable = true;
        subscriber = new TextInputSubscriber(this);
    }

    /// <inheritdoc />
    protected override bool IsContentDirty() => contentDirty;

    /// <inheritdoc />
    protected override void OnMeasure(Vector2 availableSize)
    {
        var limits = Layout.GetLimits(availableSize);
        RecomputeLines(limits.X);
        contentDirty = false;
        var contentHeight = Math.Max(lines.Count, 1) * Font.LineSpacing;
        ContentSize = Layout.Resolve(availableSize, () => new Vector2(limits.X, contentHeight));
    }

    /// <inheritdoc />
    protected override void OnDrawContent(ISpriteBatch b)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (line.Length == 0)
            {
                continue;
            }
            var lineText = text[line.StartIndex..line.EndIndex];
            b.DrawString(Font, lineText, new Vector2(0, i * Font.LineSpacing), TextColor);
        }

        if (isCapturingInput && caretVisible)
        {
            var caretPos = GetCaretPixelPosition();
            b.Draw(
                Game1.staminaRect,
                new Rectangle((int)caretPos.X, (int)caretPos.Y, 2, Font.LineSpacing),
                null,
                TextColor
            );
        }
    }

    /// <inheritdoc />
    public override void OnUpdate(TimeSpan elapsed)
    {
        base.OnUpdate(elapsed);
        if (!isCapturingInput)
        {
            return;
        }
        caretBlinkTimer += elapsed.TotalSeconds;
        if (caretBlinkTimer >= CaretBlinkIntervalSeconds)
        {
            caretBlinkTimer = 0;
            caretVisible = !caretVisible;
        }
    }

    /// <inheritdoc />
    public override void OnClick(ClickEventArgs e)
    {
        if (!e.IsPrimaryButton())
        {
            return;
        }
        Capture();
        MoveCaretToCursor(e.Position);
        e.Handled = true;
    }

    /// <inheritdoc />
    public override void OnButtonPress(ButtonEventArgs e)
    {
        if (!isCapturingInput)
        {
            base.OnButtonPress(e);
            return;
        }

        switch (e.Button)
        {
            case SButton.Left:
                CaretPosition--;
                break;
            case SButton.Right:
                CaretPosition++;
                break;
            case SButton.Up:
                MoveCaretVertically(-1);
                break;
            case SButton.Down:
                MoveCaretVertically(1);
                break;
            case SButton.Home:
                CaretPosition = lines[GetLinePosition(CaretPosition).Line].StartIndex;
                break;
            case SButton.End:
                CaretPosition = lines[GetLinePosition(CaretPosition).Line].EndIndex;
                break;
            case SButton.Delete:
                ForwardDelete();
                break;
            default:
                base.OnButtonPress(e);
                return;
        }

        e.Handled = true;
    }

    private void Capture()
    {
        isCapturingInput = true;
        caretVisible = true;
        caretBlinkTimer = 0;
        subscriber.Selected = true;
        Game1.keyboardDispatcher.Subscriber = subscriber;
    }

    private void Release()
    {
        isCapturingInput = false;
        caretVisible = false;
        subscriber.Selected = false;
        if (Game1.keyboardDispatcher.Subscriber == subscriber)
        {
            Game1.keyboardDispatcher.Subscriber = null;
        }
    }

    private void ResetCaretBlink()
    {
        caretBlinkTimer = 0;
        caretVisible = true;
    }

    // --- Text editing -------------------------------------------------

    private void InsertText(string insertion)
    {
        if (string.IsNullOrEmpty(insertion))
        {
            return;
        }
        var newText = text[..caretPosition] + insertion + text[caretPosition..];
        if (MaxLength > 0 && newText.Length > MaxLength)
        {
            return;
        }
        text = newText;
        caretPosition += insertion.Length;
        contentDirty = true;
        ResetCaretBlink();
        OnPropertyChanged(nameof(Text));
        TextChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Backspace()
    {
        if (caretPosition == 0)
        {
            return;
        }
        text = text[..(caretPosition - 1)] + text[caretPosition..];
        caretPosition--;
        contentDirty = true;
        ResetCaretBlink();
        OnPropertyChanged(nameof(Text));
        TextChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ForwardDelete()
    {
        if (caretPosition >= text.Length)
        {
            return;
        }
        text = text[..caretPosition] + text[(caretPosition + 1)..];
        contentDirty = true;
        ResetCaretBlink();
        OnPropertyChanged(nameof(Text));
        TextChanged?.Invoke(this, EventArgs.Empty);
    }

    // --- Word wrap ------------------------------------------------------
    // Greedily packs words onto each line, splitting only on existing spaces. A single word wider than the
    // available width is not broken mid-word; it will simply overflow. Good enough for prose; if you need
    // character-level breaking for very narrow boxes, that would be the next enhancement.

    private readonly record struct LineInfo(int StartIndex, int EndIndex)
    {
        public int Length => EndIndex - StartIndex;
    }

    private void RecomputeLines(float wrapWidth)
    {
        lines.Clear();
        var paragraphStart = 0;
        for (var i = 0; i <= text.Length; i++)
        {
            if (i == text.Length || text[i] == '\n')
            {
                WrapParagraph(paragraphStart, i, wrapWidth);
                paragraphStart = i + 1;
            }
        }
        if (lines.Count == 0)
        {
            lines.Add(new LineInfo(0, 0));
        }
    }

    private void WrapParagraph(int start, int end, float wrapWidth)
    {
        if (start == end)
        {
            lines.Add(new LineInfo(start, end));
            return;
        }

        var lineStart = start;
        var lastBreak = -1;          // index just after the latest space on this line

        for (var i = start; i < end; i++)
        {
            if (text[i] == ' ')
            {
                lastBreak = i + 1;
            }

            // Trailing spaces don't count toward width, so a space at the edge never forces a wrap.
            var width = Font.MeasureString(text[lineStart..(i + 1)]).X;
            if (width <= wrapWidth)
            {
                continue;
            }

            // Char i doesn't fit. Where do we cut?
            int cut;
            if (lastBreak > lineStart)
            {
                cut = lastBreak;                        // after the last space: the word moves down
            }
            else
            {
                cut = Math.Max(i, lineStart + 1);       // one word wider than the line: split it
            }

            lines.Add(new LineInfo(lineStart, cut));
            lineStart = cut;
            lastBreak = -1;
            i = cut - 1;                                // rescan from the new line's start
        }
        lines.Add(new LineInfo(lineStart, end));
    }

    // --- Caret <-> pixel/line mapping -----------------------------------

    private (int Line, int Column) GetLinePosition(int position)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (position >= line.StartIndex && position <= line.EndIndex)
            {
                return (i, position - line.StartIndex);
            }
        }
        var last = lines[^1];
        return (lines.Count - 1, last.Length);
    }

    private Vector2 GetCaretPixelPosition()
    {
        var (lineIndex, column) = GetLinePosition(caretPosition);
        var line = lines[lineIndex];
        var prefix = text[line.StartIndex..(line.StartIndex + column)];
        var x = Font.MeasureString(prefix).X;
        var y = lineIndex * Font.LineSpacing;
        return new Vector2(x, y);
    }

    private void MoveCaretToCursor(Vector2 localPosition)
    {
        if (lines.Count == 0)
        {
            return;
        }
        var lineIndex = Math.Clamp((int)MathF.Floor(localPosition.Y / Font.LineSpacing), 0, lines.Count - 1);
        var line = lines[lineIndex];
        var lineText = text[line.StartIndex..line.EndIndex];
        var column = FindColumnForX(lineText, localPosition.X);
        CaretPosition = line.StartIndex + column;
    }

    private void MoveCaretVertically(int direction)
    {
        var (currentLine, _) = GetLinePosition(caretPosition);
        var targetLine = currentLine + direction;
        if (targetLine < 0 || targetLine >= lines.Count)
        {
            return;
        }
        var targetX = GetCaretPixelPosition().X;
        var line = lines[targetLine];
        var lineText = text[line.StartIndex..line.EndIndex];
        var column = FindColumnForX(lineText, targetX);
        CaretPosition = line.StartIndex + column;
    }

    // Simple linear scan; fine for letter-length text. A production version handling very long single lines
    // could switch to the binary-search technique StardewUI's own TextInput uses.
    private int FindColumnForX(string lineText, float x)
    {
        var accumulatedWidth = 0f;
        for (var c = 0; c < lineText.Length; c++)
        {
            var charWidth = Font.MeasureString(lineText[c].ToString()).X;
            if (accumulatedWidth + charWidth / 2f > x)
            {
                return c;
            }
            accumulatedWidth += charWidth;
        }
        return lineText.Length;
    }

    // --- Keyboard capture -------------------------------------------------
    // Handles actual character insertion (typed letters, backspace, Enter-for-newline). Arrow keys, Home/End
    // and Delete are instead handled in OnButtonPress above, since that's the documented, focus-aware way
    // StardewUI routes both keyboard and gamepad button presses to a focused view.
    //
    // Note: unlike StardewUI's own TextInput, this does not implement the on-screen virtual keyboard path for
    // gamepad users (Game1.showTextEntry). Add that as a follow-up if controller support matters for your mod.

    private class TextInputSubscriber : ICaptureTarget, IKeyboardSubscriber
    {
        private readonly MultilineTextInput owner;
        
        public TextInputSubscriber(MultilineTextInput owner)
        {
            this.owner = owner;
        }

        public bool Selected { get; set; }

        public IView CapturingView => owner;

        public void RecieveCommandInput(char command) => Insert(command);

        public void RecieveSpecialInput(Keys key) { }

        public void RecieveTextInput(char inputChar) => Insert(inputChar);

        public void RecieveTextInput(string text)
        {
            if (Selected)
            {
                owner.InsertText(text);
            }
        }

        public void ReleaseCapture() => owner.Release();

        private void Insert(char c)
        {
            if (!Selected)
            {
                return;
            }
            switch (c)
            {
                case '\b':
                    owner.Backspace();
                    break;
                case '\r':
                    owner.InsertText("\n");
                    break;
                case '\t':
                    break; // Ignored for now.
                default:
                    if (!char.IsControl(c))
                    {
                        owner.InsertText(c.ToString());
                    }
                    break;
            }
        }
    }
}