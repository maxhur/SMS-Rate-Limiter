namespace smsRateRegulator.Models
{
  public class SmsRateRequest
  {
    public required string PhoneNumber { get; set; }
  }

  public class SmsRateResponse
  {
    public bool CanSend { get; set; }
  }
}
