namespace DirectML_ONNX_Search.VectorDB
{
    public class TextChunk
    {
        public TextChunk()
        {
            FilePath = String.Empty;
            Text = null;
            Vectors = Array.Empty<float>();
        }

        public TextChunk(TextChunk textChunk)
        {
            FilePath = textChunk.FilePath;
            Text = textChunk.Text;
            Vectors = textChunk.Vectors;
        }

        public string FilePath { get; set; }
        public string? Text { get; set; }
        public string? LongText { get; set; }
        public float[] Vectors { get; set; }
    }
}
