using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Trade : MonoBehaviour
{
	public TMP_Text Text;
	public Button CancelTrade;
	private TradeInfo tradeInfo;

	public void SetTrade(TradeInfo t, bool canCancel)
	{
		this.tradeInfo = t;
		var manager = GameObject.FindObjectOfType<PlayFabManager>();
		manager.GetUsernameByPlayfabId(t.OfferingPlayerId, (fromUser) =>
		{
			manager.GetUsernameByPlayfabId(t.AllowedPlayerIds[0], (toUser) =>
			{
				Text.text = $"{t.TradeId} - {fromUser} -> {toUser} - {string.Join(", ", t.OfferedCatalogItemIds)} -> {string.Join(", ", t.RequestedCatalogItemIds)}";
			});
		});

		CancelTrade.gameObject.SetActive(canCancel);
		CancelTrade.onClick.RemoveAllListeners();
		CancelTrade.onClick.AddListener(() =>
		{
			manager.CancelTrade(tradeInfo.TradeId);
		});
	}
}
