using System.Threading.Tasks;
using Unity.Services.Multiplay;
using UnityEngine;

public class MultiplayServiceHandler
{
    private MultiplayEventCallbacks _callbacks;

    public MultiplayServiceHandler()
    {
        var callbacks = new MultiplayEventCallbacks();
        callbacks.Allocate += OnAllocate;
        callbacks.Deallocate += OnDeallocate;
        callbacks.Error += OnError;
        callbacks.SubscriptionStateChanged += OnSubscriptionStateChanged;
    }

    public async Task<IServerEvents> SubscribeToEvents()
    {
        while (MultiplayService.Instance == null)
        {
            await Awaitable.NextFrameAsync();
        }

        return await MultiplayService.Instance.SubscribeToServerEventsAsync(_callbacks);
    }

    private void OnSubscriptionStateChanged(MultiplayServerSubscriptionState obj)
    {
        Debug.Log($"Subscription state changed: {obj}");
    }

    private void OnError(MultiplayError obj)
    {
        Debug.Log($"Error received: {obj}");
    }

    private async void OnDeallocate(MultiplayDeallocation obj)
    {
        Debug.Log($"Deallocation received: {obj}");
        await MultiplayService.Instance.UnreadyServerAsync();
    }

    private async void OnAllocate(MultiplayAllocation allocation)
    {
        Debug.Log($"Allocation received: {allocation}");
        await MultiplayService.Instance.ReadyServerForPlayersAsync();
    }
} 