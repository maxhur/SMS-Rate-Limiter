namespace smsRateRegulator.Models
{
  public class SmsRateRequest
  {
    public string PhoneNumber { get; set; } = string.Empty;
    public int PerNumberLimitPerSecond { get; set; }
    public int PerAccountLimitPerSecond { get; set; }
  }

  public class SmsRateResponse
  {
    public bool CanSend { get; set; }
  }
}
