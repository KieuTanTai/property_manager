namespace Payment.Models.Payment
{
    /// <summary>
    /// VNPAY IPN acknowledgement. <see cref="RspCode"/> follows the VNPAY protocol:
    /// 00 = confirmed, 01 = order not found, 04 = invalid amount, 97 = invalid signature,
    /// 99 = unprocessed (VNPAY will retry).
    /// </summary>
    public sealed record IpnResult(int RspCode, string Message);
}
