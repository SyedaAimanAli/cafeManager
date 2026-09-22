using System;

namespace RollUp.Core.Interfaces;

public interface IOrderNotificationService
{
    event Action OnOrdersChanged;
    void NotifyOrdersChanged();
}
