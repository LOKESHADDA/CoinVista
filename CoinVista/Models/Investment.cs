namespace CoinVista.Models;
public class Investment {
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string CryptoId { get; set; } = "";
    public string CryptoName { get; set; } = "";
    public decimal AmountInvested { get; set; }
    public decimal Quantity { get; set; }
    public decimal PurchasePrice { get; set; }   // price at investment time
    public DateTime InvestmentDate { get; set; }
}
