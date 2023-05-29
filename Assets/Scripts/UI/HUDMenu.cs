using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public record PopupData
    {
        public readonly string title;
        public readonly Sprite icon;
        
        public PopupData(string title, Sprite icon)
        {
            this.title = title;
            this.icon = icon;
        }
    }
    
    public record ProgressData
    {
        public readonly string title;
        public readonly Sprite icon;
        public readonly float progress;
        public readonly float maxProgress;
        
        public ProgressData(string title,  Sprite icon, float progress, float maxProgress)
        {
            this.title = title;
            this.icon = icon;
            this.maxProgress = maxProgress;
            this.progress = progress;
        }
    }
    
    public enum PopupState
    {
        Open,
        Fading,
        Closed
    }
    
    public class HUDMenu : MonoBehaviour
    {
        public float popupDuration = 3f;
        public float popupFadeDuration = 0.5f;

        private bool _isPopupOpen;
        private bool _isProgressOpen;
        private readonly Queue<PopupData> _popupQueue = new();
        private ProgressData _nextProgress;
        private PopupData _currentPopup;
        private ProgressData _currentProgress;
        private float _popupTimer;
        private float _progressTimer;
        private PopupState _popupState = PopupState.Closed;
        private PopupState _progressState = PopupState.Closed;

        private VisualElement _root;
        
        private VisualElement _popup;
        private VisualElement _popupIcon;
        private Label _popupLabel;

        private VisualElement _progress;
        private VisualElement _progressIcon;
        private Label _progressLabel;
        private ProgressBar _progressBar;

        private void Start()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _popup = _root.Q<VisualElement>("Popup");
            _popupIcon = _popup.Q<VisualElement>("Icon");
            _popupLabel = _popup.Q<Label>("PopupLabel");
            
            _progress = _root.Q<VisualElement>("ItemCounter");
            _progressIcon = _progress.Q<VisualElement>("Icon");
            _progressLabel = _progress.Q<Label>("CounterLabel");
            _progressBar = _progress.Q<ProgressBar>("ItemCounterProgress");
            
            _popupTimer = popupDuration;
            _progressTimer = popupDuration;
        }

        private void Update()
        {
            _popupTimer += Time.deltaTime;
            _progressTimer += Time.deltaTime;

            switch (_popupState)
            {
                case PopupState.Open:
                    if(_popupTimer >= popupDuration)
                    {
                        _popupState = PopupState.Fading;
                        _popupTimer = 0f;
                        
                        FadePopup();
                    }
                    break;
                case PopupState.Fading:
                    if (_popupTimer >= popupFadeDuration)
                    {
                        bool opened = CheckAndOpenPopup();
                        if (opened)
                        {
                            _popupState = PopupState.Open;
                            _popupTimer = 0f;
                        }
                        else
                        {
                            _popupState = PopupState.Closed;
                            
                            ClosePopup();
                        }
                    }
                    break;
                case PopupState.Closed:
                    bool closedOpened = CheckAndOpenPopup();
                    
                    if (closedOpened)
                    {
                        _popupState = PopupState.Open;
                        _popupTimer = 0f;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            bool progressOpened = CheckAndOpenProgress();
            if (progressOpened)
            {
                _progressState = PopupState.Open;
                _progressTimer = 0f;
            }

            switch (_progressState)
            {
                case PopupState.Open:
                    if(_progressTimer >= popupDuration) 
                    {
                        _progressState = PopupState.Fading;
                        _progressTimer = 0f;
                        
                        FadeProgress();
                    }
                    break;
                case PopupState.Fading:
                    if (_progressTimer >= popupFadeDuration)
                    {
                        _progressTimer = 0f;
                        _progressState = PopupState.Closed;
                        
                        CloseProgress();
                    }
                    break;
                case PopupState.Closed:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool CheckAndOpenPopup()
        {
            if (_popupQueue.Count == 0)
            {
                return false;
            }
            
            _currentPopup = _popupQueue.Dequeue();

            _popupIcon.style.backgroundImage = new StyleBackground(_currentPopup.icon);
            _popupLabel.text = _currentPopup.title;
            _popup.AddToClassList("open");
            _isPopupOpen = true;
            
            return true;
        }

        private bool CheckAndOpenProgress()
        {
            if (_nextProgress == null)
            {
                return false;
            }
            
            _currentProgress = _nextProgress;
            _nextProgress = null;
            
            _progressIcon.style.backgroundImage = new StyleBackground(_currentProgress.icon);
            _progressLabel.text = _currentProgress.title;
            _progressBar.highValue = _currentProgress.maxProgress;
            _progressBar.value = _currentProgress.progress;
            _progress.AddToClassList("open");
            _isProgressOpen = true;
            
            return true;
        }

        private void FadePopup()
        {
            _popup.RemoveFromClassList("open");
        }
        
        private void FadeProgress()
        {
            _progress.RemoveFromClassList("open");
        }

        private void UnfadePopup()
        {
            _popup.AddToClassList("open");
            _popupState = PopupState.Open;
            _popupTimer = 0f;
        }

        private void UnfadeProgress()
        {
            _progress.AddToClassList("open");
            _progressState = PopupState.Open;
            _progressTimer = 0f;
        }
        
        private void ClosePopup()
        {
            if (!_isPopupOpen)
            {
                return;
            }

            _isPopupOpen = false;
            _popup.RemoveFromClassList("open");
            _currentPopup = null;
        }

        private void CloseProgress()
        {
            if (!_isProgressOpen)
            {
                return;
            }
            
            _progress.RemoveFromClassList("open");
            _isProgressOpen = false;
            _currentProgress = null;
        }
        
        public void QueuePopup(PopupData popupData)
        {
            if (popupData.title == _currentPopup?.title)
            {
                UnfadePopup();
                return;
            }
            
            _popupQueue.Enqueue(popupData);
        }

        public void ShowProgress(ProgressData progressData)
        {
            if (progressData.title == _currentProgress?.title)
            {
                _progressBar.value = progressData.progress;
                _progressBar.highValue = progressData.maxProgress;
                UnfadeProgress();
                return;
            }
            
            _nextProgress = progressData;
        }
    }
}