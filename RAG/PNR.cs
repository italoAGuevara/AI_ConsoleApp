using Microsoft.Extensions.VectorData;

namespace AI_ConsoleApp_Ollama_RAG
{
    public class PNR
    {
        [VectorStoreRecordKey]
        public int Key { get; set; }
                
        [VectorStoreRecordData]
        public string Record { get; set; }

        [VectorStoreRecordData]
        public int Index { get; set; }

        [VectorStoreRecordData]
        public string CreatredBy { get; set; }

        [VectorStoreRecordVector(384, DistanceFunction.CosineSimilarity)]
        public ReadOnlyMemory<float> Vector { get; set; }
    }
}
