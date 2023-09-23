/**
 * Copyright (c) 2020-2021 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using UnityEngine;
using UnityEngine.UI;
using Simulator.Database.Services;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Simulator.Web
{
    public class ConnectionUI : MonoBehaviour
    {
        public GameObject statusMenuRoot;
        public GameObject dropdownArrow;
        public Text statusText;
        public Button statusButton;
        public Text statusButtonText;
        public Image statusButtonIcon;
        public Button statusMenuButton;
        public Text statusMenuButtonText;
        public Button unlinkButton;
        public Button linkButton;
        public Button clearAssetCacheButton;
        public Button quitButton;
        public Text linkButtonText;
        public static ConnectionUI instance;
        public Color offlineColor;
        public Color onlineColor;
        public Dropdown offlineDropdown;
        public Button offlineStartButton;
        public Button offlineStopButton;
        public Text CloudTypeText;
        public Button VSEButton;
        public CacheControlWindow CacheControlWindow;

        public enum LoaderUIStateType { START, PROGRESS, READY };
        public LoaderUIStateType LoaderUIState = LoaderUIStateType.START;

        private SimulationService simulationService = new SimulationService();
        private List<SimulationData> simulationData;
        private int selectedSim;

        public void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            ColorUtility.TryParseHtmlString("#1F2940", out offlineColor);
            ColorUtility.TryParseHtmlString("#FFFFFF", out onlineColor);
            statusButtonIcon.material.color = Color.white;
            instance = this;
            statusButton.onClick.AddListener(OnStatusButtonClicked);
            statusMenuButton.onClick.AddListener(OnStatusMenuButtonClicked);
            linkButton.onClick.AddListener(OnLinkButtonClicked);
            offlineStartButton.onClick.AddListener(OnOfflineStartButtonClicked);
            offlineStopButton.onClick.AddListener(OnOfflineStopButtonClicked);
            clearAssetCacheButton.onClick.AddListener(() =>
            {
                CacheControlWindow.gameObject.SetActive(true);
            });
            unlinkButton.onClick.AddListener(OnUnlinkButtonClicked);
            quitButton.onClick.AddListener(OnQuitButtonClicked);
            UpdateDropdown();
            offlineDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            UpdateStatus();
            TaskProgressManager.Instance.OnUpdate += UpdateDownloadProgress;
            if (Config.SimulationID != null)
            {
                _ = StartSimulation(Config.SimulationID);
            }
        }

        async Task StartSimulation(string simulationID)
        {
            //await new WaitForSeconds(3.0f);
            Console.WriteLine($"SimulationID is {simulationID}:\n");
            await Task.Delay(5000);
            selectedSim = int.Parse(simulationID);
            OnOfflineStartButtonClicked();
        }

        public void UpdateDownloadProgress()
        {
            var text = string.Empty;
            foreach (var item in TaskProgressManager.Instance.Tasks)
            {
                text += $"{item.Description} {Mathf.Floor(item.Progress * 100)}%\n";
            }
            if (statusText != null)
                statusText.text = text;
        }

        public void UpdateStatus()
        {
            if (statusText == null || linkButton == null || statusButtonIcon == null || statusButtonText == null || linkButtonText == null || statusButton == null)
                return; // fix for editor stop playmode null

            switch (ConnectionManager.Status)
            {
                case ConnectionManager.ConnectionStatus.Connecting:
                    statusText.text = $"Connecting to the cloud...";
                    unlinkButton.interactable = false;
                    linkButton.gameObject.SetActive(false);
                    statusButtonIcon.color = offlineColor;
                    offlineDropdown.gameObject.SetActive(false);
                    offlineStartButton.gameObject.SetActive(false);
                    VSEButton.gameObject.SetActive(false);
                    CloudTypeText.text = ConnectionManager.API?.CloudType;
                    break;
                case ConnectionManager.ConnectionStatus.Connected:
                    statusText.text = "";
                    statusButtonText.text = "Online";
                    statusMenuButtonText.text = "Go Offline";
                    statusButtonIcon.color = offlineColor;
                    linkButtonText.text = "LINK TO CLOUD";
                    unlinkButton.interactable = false;
                    linkButton.gameObject.SetActive(true);
                    offlineDropdown.gameObject.SetActive(false);
                    offlineStartButton.gameObject.SetActive(false);
                    VSEButton.gameObject.SetActive(true);
                    CloudTypeText.text = ConnectionManager.API?.CloudType;
                    break;
                case ConnectionManager.ConnectionStatus.Offline:
                    statusButtonText.text = "Offline";
                    statusMenuButtonText.text = "Go Online";
                    if (ConnectionManager.DisconnectReason != null)
                    {
                        statusText.text = "Disconnected: " + ConnectionManager.DisconnectReason;
                    }
                    else
                    {
                        statusText.text = "Go Online to start new simulation or run previous simulations while being Offline";
                    }
                    statusButtonIcon.color = offlineColor;
                    unlinkButton.interactable = true;
                    linkButton.gameObject.SetActive(false);
                    offlineDropdown.gameObject.SetActive(true);
                    offlineStartButton.gameObject.SetActive(true);
                    VSEButton.gameObject.SetActive(true);
                    UpdateDropdown();
                    CloudTypeText.text = "OFFLINE";
                    break;
                case ConnectionManager.ConnectionStatus.Online:
                    statusButtonText.text = "Online";
                    statusMenuButtonText.text = "Go Offline";
                    statusButtonIcon.color = onlineColor;
                    statusText.text = "";
                    linkButtonText.text = "OPEN BROWSER";
                    unlinkButton.interactable = true;
                    linkButton.gameObject.SetActive(true);
                    offlineDropdown.gameObject.SetActive(false);
                    offlineStartButton.gameObject.SetActive(false);
                    VSEButton.gameObject.SetActive(true);
                    CloudTypeText.text = ConnectionManager.API?.CloudType;
                    break;
            }
        }

        public void UpdateStatusText(string text)
        {
            statusText.text = text;
        }

        public void UpdateDropdown()
        {
            simulationData = simulationService.List().ToList();
            offlineDropdown.ClearOptions();
            offlineDropdown.AddOptions(simulationData.Select(s => s.Name).ToList());
            offlineDropdown.value = 0;
            selectedSim = 0;
            if (simulationData.Count == 0)
            {
                offlineDropdown.gameObject.SetActive(false);
                offlineStartButton.gameObject.SetActive(false);
            }
        }

        public void OnDropdownValueChanged(int value)
        {
            selectedSim = value;
        }

        public void OnStatusButtonClicked()
        {
            bool active = !statusMenuRoot.gameObject.activeSelf;
            statusMenuRoot.SetActive(active);
            dropdownArrow.transform.localScale = new Vector3(1, active ? 1 : -1, 1);
        }

        public void OnQuitButtonClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
             Application.Quit();
#endif
        }

        public void OnOfflineStartButtonClicked()
        {
            Loader.Instance.StartSimulation(simulationData[selectedSim]);
            if (simulationData[selectedSim].ApiOnly)
            {
                offlineStopButton.gameObject.SetActive(true);
            }
        }

        public void OnOfflineStopButtonClicked()
        {
            Loader.Instance.StopAsync();
        }

        public void SetLinkingButtonActive(bool active)
        {
            linkButton.gameObject.SetActive(active);
        }

        public void SetVSEButtonActive(bool active)
        {
            VSEButton.gameObject.SetActive(active);
        }

        public void OnStatusMenuButtonClicked()
        {
            statusMenuRoot.gameObject.SetActive(false);
            ConnectionManager.instance.ConnectionStatusEvent();
        }

        public void OnLinkButtonClicked()
        {
            if (ConnectionManager.Status == ConnectionManager.ConnectionStatus.Connected)
            {
                Application.OpenURL(ConnectionManager.instance.LinkUrl);
            }
            else if (ConnectionManager.Status == ConnectionManager.ConnectionStatus.Online)
            {
                Application.OpenURL(Simulator.Web.Config.CloudUrl);
            }
        }

        public void SetLoaderUIState(LoaderUIStateType state)
        {
            LoaderUIState = state;
            switch (LoaderUIState)
            {
                case LoaderUIStateType.START:
                    break;
                case LoaderUIStateType.PROGRESS:
                    statusButtonText.text = "Loading...";
                    statusText.text = "Loading...";
                    break;
                case LoaderUIStateType.READY:
                    statusButtonText.text = "API ready!";
                    statusText.text = "API ready!";
                    break;
            }
        }

        public void OnUnlinkButtonClicked()
        {
            statusMenuRoot.SetActive(false);
            if (ConnectionManager.Status == ConnectionManager.ConnectionStatus.Online)
            {
                ConnectionManager.instance.Disconnect();
            }
            Config.RegenerateSimID();
        }

        public void EnterScenarioEditor()
        {
            Loader.Instance.EnterScenarioEditor();
        }
    }
}
