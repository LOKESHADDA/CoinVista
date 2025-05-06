namespace CoinVista.Models;
public class CoinDetails {
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal CurrentPrice { get; set; }
    // (optional) public Dictionary<string, decimal> PricesLast7Days { get; set; }
}
