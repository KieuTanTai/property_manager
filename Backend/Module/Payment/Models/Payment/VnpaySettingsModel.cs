namespace Payment.Models.Payment
{
    /// <summary>
    /// VNPAY configuration bound from the <c>VNPAY</c> configuration section.
    /// </summary>
    public sealed class VnpaySettingsModel
    {
        public string TmnCode { get; init; } = string.Empty;

        public string HashSecret { get; init; } = string.Empty;

        public string BaseUrl { get; init; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

        public string CallbackUrl { get; init; } = string.Empty;

        public string Version { get; init; } = "2.1.0";

        public string OrderType { get; init; } = "other";
    }
}
