namespace smsRateRegulator.Models
{
  public class SmsRateRequest
  {
    public required string PhoneNumber { get; set; }
    public required int maxPerPhoneNumberPerSecond { get; set; }
    public required int maxPerAccountPerSecond { get; set; }
  }

  public class SmsRateResponse
  {
    public bool CanSend { get; set; }
  }
}
