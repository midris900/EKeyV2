namespace EKeyAuthenticationService.Model
{
    public class InitiatePaymentModel
    {
        required public string EntityId { get; set; }
        required public string OfficialId { get; set; }
        required public string OfficialIdType { get; set; }
        required public string ServiceCode { get; set; }
        required public string PaymentAmount { get; set; }
        required public string RequestNumber { get; set; }
    }
}
