using CluedIn.Core.Data;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.ExternalSearch.Providers.Openfigi.Vocabularies
{
    public class OpenfigiVocabulary : SimpleVocabulary
    {
        public OpenfigiVocabulary()
        {
            this.VocabularyName = "Openfigi Organization";
            this.KeyPrefix = "openfigi.Organization";
            this.KeySeparator = ".";
            this.Grouping = EntityType.Organization;

            this.AddGroup("Openfigi Organization Details", group =>
            {
                this.Figi = group.Add(new VocabularyKey("AddressComponents", VocabularyKeyDataType.Json, VocabularyKeyVisibility.Hidden));
                this.Name = group.Add(new VocabularyKey("AddressComponents", VocabularyKeyDataType.Json, VocabularyKeyVisibility.Hidden));
                this.Ticker = group.Add(new VocabularyKey("AddressComponents", VocabularyKeyDataType.Json, VocabularyKeyVisibility.Hidden));
                this.ExchCode = group.Add(new VocabularyKey("AddressComponents", VocabularyKeyDataType.Json, VocabularyKeyVisibility.Hidden));
                this.CompositeFIGI = group.Add(new VocabularyKey("AddressComponents", VocabularyKeyDataType.Json, VocabularyKeyVisibility.Hidden));
                this.UniqueID = group.Add(new VocabularyKey("AddressComponents", VocabularyKeyDataType.Json, VocabularyKeyVisibility.Hidden));
                this.SecurityType = group.Add(new VocabularyKey("AddressComponents", VocabularyKeyDataType.Json, VocabularyKeyVisibility.Hidden));
                this.MarketSector = group.Add(new VocabularyKey("AddressComponents", VocabularyKeyDataType.Json, VocabularyKeyVisibility.Hidden));
                this.ShareClassFIGI = group.Add(new VocabularyKey("AddressComponents", VocabularyKeyDataType.Json, VocabularyKeyVisibility.Hidden));
                this.UniqueIDFutOpt = group.Add(new VocabularyKey("AddressComponents", VocabularyKeyDataType.Json, VocabularyKeyVisibility.Hidden));
                this.SecurityType2 = group.Add(new VocabularyKey("AddressComponents", VocabularyKeyDataType.Json, VocabularyKeyVisibility.Hidden));
                this.SecurityDescription = group.Add(new VocabularyKey("AddressComponents", VocabularyKeyDataType.Json, VocabularyKeyVisibility.Hidden));


            });

        }

        public VocabularyKey Figi { get; set; } 
        public VocabularyKey Name { get; set; } 
        public VocabularyKey Ticker { get; set; } 
        public VocabularyKey ExchCode { get; set; } 
        public VocabularyKey CompositeFIGI { get; set; } 
        public VocabularyKey UniqueID { get; set; } 
        public VocabularyKey SecurityType { get; set; } 
        public VocabularyKey MarketSector { get; set; } 
        public VocabularyKey ShareClassFIGI { get; set; } 
        public VocabularyKey UniqueIDFutOpt { get; set; } 
        public VocabularyKey SecurityType2 { get; set; } 
        public VocabularyKey SecurityDescription { get; set; }
    }
}
