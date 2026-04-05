using Unity.Netcode;

namespace Sessions.Validators
{
    /// <summary>
    /// Проверяет входящие подключение.
    /// </summary>
    public interface IConnectionValidator
    {
        /// <summary>
        /// Проверяет запрос на выполнения условия.
        /// </summary>
        public bool Validate(in NetworkManager.ConnectionApprovalRequest request, out string reason);
    }
}