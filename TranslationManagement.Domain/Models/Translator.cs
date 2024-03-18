namespace TranslationManagement.Domain.Models
{
    public class Translator
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal HourlyRate { get; set; }
        public string Status { get; set; }
        public string CreditCardNumber { get; set; }
    }
}
