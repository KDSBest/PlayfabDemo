using PlayFab;
using PlayFab.ClientModels;
using PlayFab.CloudScriptModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PlayFabManager : MonoBehaviour
{
	public TMP_InputField emailText;
	public TMP_InputField passwordText;
	public TMP_Text resultText;

	public GameObject LoginScreen;
	public GameObject GameScreen;

	public List<ItemInstance> Inventory;
	public Transform InventoryParent;
	public GameObject ItemPrefab;
	public TMP_InputField tradeEmailText;
	public List<GameObject> Items = new List<GameObject>();
	public List<GameObject> Trades = new List<GameObject>();
	public Transform OpenedTrades;
	public Transform AcceptedTrades;
	public GameObject TradePrefab;

	public void Login()
	{
		var req = new LoginWithEmailAddressRequest()
		{
			Email = emailText.text,
			Password = passwordText.text
		};
		PlayFabClientAPI.LoginWithEmailAddress(req, OnLoginSuccess, OnError);
	}

	public void AcceptTrade(string itemInstanceId, string id, string email)
	{
		this.GetPlayfabIdByEmail(email, (playerId) =>
		{
			PlayFabClientAPI.AcceptTrade(new AcceptTradeRequest()
			{
				TradeId = id,
				OfferingPlayerId = playerId,
				AcceptedInventoryInstanceIds = new List<string>() { itemInstanceId }
			}, OnAcceptTrade, OnError);
		});
	}

	private void OnAcceptTrade(AcceptTradeResponse data)
	{
		UpdateMyData();
	}

	public void CancelTrade(string tradeId)
	{
		PlayFabClientAPI.CancelTrade(new CancelTradeRequest()
		{
			TradeId = tradeId
		}, OnCancelTrade, OnError);
	}

	private void OnCancelTrade(CancelTradeResponse obj)
	{
		UpdateMyData();
	}

	public void Register()
	{
		var req = new RegisterPlayFabUserRequest()
		{
			Username = emailText.text.Split('@')[0],
			Email = emailText.text,
			Password = passwordText.text
		};
		PlayFabClientAPI.RegisterPlayFabUser(req, OnRegisterSuccess, OnError);
	}

	public void GetUsernameByPlayfabId(string id, Action<string> callback)
	{
		PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest()
		{
			PlayFabId = id
		}, (res) =>
		{
			callback(res.AccountInfo.Username);
		}, OnError);
	}

	public void GetPlayfabIdByEmail(string email, Action<string> callback)
	{
		PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest()
		{
			Email = email
		}, (res) =>
		{
			callback(res.AccountInfo.PlayFabId);
		}, OnError);
	}

	public void OpenTrade(string instanceId)
	{
		GetPlayfabIdByEmail(tradeEmailText.text, (id) =>
		{
			PlayFabClientAPI.OpenTrade(new OpenTradeRequest()
			{
				AllowedPlayerIds = new List<string>() { id },
				OfferedInventoryInstanceIds = new List<string>() { instanceId },
				RequestedCatalogItemIds = new List<string>() { Inventory.First(x => x.ItemInstanceId == instanceId).ItemId == "One" ? "Two" : "One" }
			}, OnOpenTradeSuccessful, OnError);
		});
	}

	private void OnOpenTradeSuccessful(OpenTradeResponse data)
	{
		UpdateMyData();
	}

	public void GetMyTrades()
	{
		foreach (var go in Trades)
		{
			GameObject.Destroy(go);
		}
		Trades.Clear();
		
		PlayFabClientAPI.GetPlayerTrades(new GetPlayerTradesRequest()
		{
		 	
		}, OnTrades, OnError);

	}

	private void OnTrades(GetPlayerTradesResponse data)
	{
		foreach (var t in data.OpenedTrades)
		{
			if (t.Status == TradeStatus.Cancelled || t.Status == TradeStatus.Invalid)
				continue;

			var tGo = GameObject.Instantiate(TradePrefab, OpenedTrades);
			tGo.GetComponent<Trade>().SetTrade(t, true);
			Trades.Add(tGo);
		}
		foreach (var t in data.AcceptedTrades)
		{
			var tGo = GameObject.Instantiate(TradePrefab, AcceptedTrades);
			tGo.GetComponent<Trade>().SetTrade(t, false);
			Trades.Add(tGo);
		}
	}

	public void GetMyItems()
	{
		foreach(var go in Items)
		{
			GameObject.Destroy(go);
		}
		Items.Clear();

		PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest()
		{

		}, OnInventory, OnError);

	}

	private void OnInventory(GetUserInventoryResult data)
	{
		Inventory = data.Inventory;
		foreach (var item in Inventory)
		{
			var itemGo = GameObject.Instantiate(ItemPrefab, InventoryParent);
			itemGo.GetComponent<Item>().SetItem(item);
			Items.Add(itemGo);
		}
	}

	public void CallFunctionApp()
	{
		PlayFabCloudScriptAPI.ExecuteFunction(new ExecuteFunctionRequest()
		{
			Entity = new PlayFab.CloudScriptModels.EntityKey()
			{
				Id = PlayFabSettings.staticPlayer.EntityId,
				Type = PlayFabSettings.staticPlayer.EntityType
			},
			FunctionName = "PlayFabDemo",
			FunctionParameter = new Dictionary<string, object>() { { "inputValue", "Test" } },
			GeneratePlayStreamEvent = true
		}, (ExecuteFunctionResult result) =>
		{
			if (result.FunctionResultTooLarge ?? false)
			{
				Debug.Log("This can happen if you exceed the limit that can be returned from an Azure Function, See PlayFab Limits Page for details.");
				return;
			}
			Debug.Log($"The {result.FunctionName} function took {result.ExecutionTimeMilliseconds} to complete");
			Debug.Log($"Result: {result.FunctionResult.ToString()}");
			UpdateMyData();
		}, (PlayFabError error) =>
		{
			Debug.Log($"Opps Something went wrong: {error.GenerateErrorReport()}");
		});
	}

	private void OnRegisterSuccess(RegisterPlayFabUserResult obj)
	{
		resultText.text = "Register successful.";
	}

	private void OnLoginSuccess(LoginResult data)
	{
		resultText.text = "Login successful.";
		LoginScreen.SetActive(false);
		UpdateMyData();
		GameScreen.SetActive(true);
	}

	private void UpdateMyData()
	{
		GetMyItems();
		GetMyTrades();
	}

	private void OnError(PlayFabError data)
	{
		resultText.text = data.ErrorMessage;
		Debug.LogError(data.ErrorMessage);
	}
}
