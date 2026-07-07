namespace SariaMod.Items.zTalking
{
    /// <summary>
    /// One section of dialogue text with its own display properties.
    /// The section editor composes these into the final tagged dialogue string.
    /// </summary>
    internal sealed class EditorTextSection
    {
        /// <summary>Raw text content (no tags — tags are generated from properties).</summary>
        public string Text = "";

        /// <summary>Named color key from DialogueUIState.NamedColors (e.g. "White", "Pink").</summary>
        public string Color = "White";

        /// <summary>Typewriter speed (frames per character). Higher = slower.</summary>
        public int Speed = 2;

        /// <summary>Whether mouth animates during this section.</summary>
        public bool Mouth = true;

        /// <summary>Pause frames inserted after the last character of this section ([wait:N]).</summary>
        public int WaitFrames = 0;

        public EditorTextSection() { }

        public EditorTextSection(string text, string color = "White", int speed = 2, bool mouth = true, int waitFrames = 0)
        {
            Text = text ?? "";
            Color = color ?? "White";
            Speed = speed;
            Mouth = mouth;
            WaitFrames = waitFrames;
        }

        /// <summary>Creates a deep copy of this section.</summary>
        public EditorTextSection Clone()
        {
            return new EditorTextSection(Text, Color, Speed, Mouth, WaitFrames);
        }
    }
}
