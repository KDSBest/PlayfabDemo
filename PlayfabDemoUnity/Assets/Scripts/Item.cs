using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
	public TMP_Text Text;
	public Button BtnOpenTrade;
	private ItemInstance item;

	public Button BtnAcceptTrade;
	public TMP_InputField TradeId;
	public TMP_InputField TradeEmail;
	public void SetItem(ItemInstance instance)
	{
		var manager = GameObject.FindObjectOfType<PlayFabManager>();
		item = instance;
		string name = string.IsNullOrEmpty(instance.DisplayName) ? instance.ItemId : instance.DisplayName;
		Text.text = $"{name} - {instance.ItemInstanceId}";
		BtnOpenTrade.onClick.RemoveAllListeners();
		BtnOpenTrade.onClick.AddListener(() =>
		{
			manager.OpenTrade(item.ItemInstanceId);
		});

		BtnAcceptTrade.onClick.RemoveAllListeners();
		BtnAcceptTrade.onClick.AddListener(() =>
		{
			manager.AcceptTrade(item.ItemInstanceId, TradeId.text, TradeEmail.text);
		});
	}
}
