using System;
using System.Collections.Generic;
using Codice.Client.BaseCommands;
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
    
    public record ProgressData : PopupData
    {
        public float progress;
        public float maxProgress;
        
        public ProgressData(string title, Sprite icon, float progress, float maxProgress) : base(title, icon)
        {
            this.maxProgress = maxProgress;
            this.progress = progress;
        }
    }

    public abstract class StateData
    {
        public readonly VisualElement popup;
        private readonly float _popupDuration;
        private readonly float _popupFadeDuration;
        
        private PopupState _state;
        private float _time;
        
        protected StateData(VisualElement popup, float popupDuration, float popupFadeDuration)
        {
            this.popup = popup;
            _popupDuration = popupDuration;
            _popupFadeDuration = popupFadeDuration;
            
            _state = PopupState.Open;
            _time = 0f;
           
            popup.AddToClassList("open");
        }
        
        public PopupState Update(float deltaTime)
        {
            _time += deltaTime;

            switch (_state)
            {
                case PopupState.Open:
                    if(_time >= _popupDuration)
                    {
                        _state = PopupState.Fading;
                        _time = 0f;
                        
                        Fade();
                    }
                    break;
                case PopupState.Fading:
                    if (_time >= _popupFadeDuration)
                    {
                        _state = PopupState.Closed;
                        
                        Close();
                    }
                    break;
                case PopupState.Closed:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return _state;
        }

        public void Unfade()
        {
            popup.AddToClassList("open");
            _state = PopupState.Open;
            _time = 0f;
        }

        private void Fade()
        {
            popup.RemoveFromClassList("open");
        }

        private void Close()
        {
            popup.RemoveFromHierarchy();
        }
    }
    
    public class PopupStateData : StateData
    {
        public readonly PopupData popupData;

        public PopupStateData(PopupData popupData, VisualElement popup, float popupDuration, float popupFadeDuration) :
            base(popup, popupDuration, popupFadeDuration)
        {
            this.popupData = popupData;

            VisualElement popupIcon = popup.Q<VisualElement>("Icon");
            Label popupLabel = popup.Q<Label>("PopupLabel");

            popupIcon.style.backgroundImage = new StyleBackground(this.popupData.icon);
            popupLabel.text = this.popupData.title;
        }
    }
    
    public class ProgressStateData : StateData
    {
        public readonly ProgressData progressData;
        private readonly ProgressBar _progressBar;

        public bool Completed => _progressBar.value >= _progressBar.highValue;
        
        public ProgressStateData(ProgressData progressData, VisualElement progress, float popupDuration, float popupFadeDuration) :
            base(progress, popupDuration, popupFadeDuration)
        {
            this.progressData = progressData;
            
            VisualElement progressIcon = progress.Q<VisualElement>("Icon");
            Label progressLabel = progress.Q<Label>("CounterLabel");
            _progressBar = progress.Q<ProgressBar>("ItemCounterProgress");
            
            progressIcon.style.backgroundImage = new StyleBackground(this.progressData.icon);
            progressLabel.text = this.progressData.title;
            _progressBar.highValue = this.progressData.maxProgress;
            _progressBar.value = this.progressData.progress;
        }

        public void UpdateProgress(float value, float highValue)
        {
            _progressBar.value = value;
            _progressBar.highValue = highValue;
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
        
        public int maxPopupCount = 5;
        
        public VisualTreeAsset popupVisualTreeAsset;
        public VisualTreeAsset progressVisualTreeAsset;

        private int _popupCount = 0;

        private readonly Queue<PopupData> _popupQueue = new Queue<PopupData>();
        
        private readonly LinkedList<PopupStateData> _popupStateData = new LinkedList<PopupStateData>();
        private readonly LinkedList<ProgressStateData> _progressStateData = new LinkedList<ProgressStateData>();

        private VisualElement _root;
        private VisualElement _popupContainer;


        private void Start()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            
            _popupContainer = _root.Q<VisualElement>("PopupContainer");
        }

        private void Update()
        {
            while (_popupCount < maxPopupCount && _popupQueue.Count > 0)
            {
                DequeuePopup();
            }
            
            if (_popupCount == 0)
            {
                return;
            }
            
            float deltaTime = Time.deltaTime;

            foreach (var popupStateData in _popupStateData)
            {
                PopupState state = popupStateData.Update(deltaTime);

                if (state == PopupState.Closed)
                {
                    _popupCount--;
                    _popupStateData.Remove(popupStateData);
                    break;
                }
            }

            foreach (var progressStateData in _progressStateData)
            {
                PopupState state = progressStateData.Update(deltaTime);

                if (state == PopupState.Closed)
                {
                    _popupCount--;
                    _progressStateData.Remove(progressStateData);
                    break;
                }
            }
        }

        private void DequeuePopup()
        {
            if (_popupQueue.Count <= 0)
            {
                return;
            }

            PopupData popupData = _popupQueue.Dequeue();

            if (popupData is ProgressData progressData)
            {
                ProgressStateData progressStateData = new ProgressStateData(progressData, progressVisualTreeAsset.CloneTree(), popupDuration, popupFadeDuration);
                    
                _progressStateData.AddLast(progressStateData);
                _popupContainer.Add(progressStateData.popup);
            }
            else
            {
                PopupStateData popupStateData = new PopupStateData(popupData, popupVisualTreeAsset.CloneTree(), popupDuration, popupFadeDuration);
                
                _popupStateData.AddLast(popupStateData);
                _popupContainer.Add(popupStateData.popup);
            }
                
            _popupCount++;
        }

        public void QueuePopup(PopupData popupData)
        {
            foreach (var popupStateData in _popupStateData)
            {
                if (popupStateData.popupData.title != popupData.title)
                {
                    continue;
                }

                popupStateData.Unfade();
                return;
            }
            
            foreach (var data in _popupQueue)
            {
                if (data is not ProgressData && data.title == popupData.title)
                {
                    return;
                }
            }

            _popupQueue.Enqueue(popupData);
        }

        public void ShowProgress(ProgressData progressData)
        {
            foreach (var progressStateData in _progressStateData)
            {
                if (progressStateData.progressData.title != progressData.title || progressStateData.Completed)
                {
                    continue;
                }

                progressStateData.UpdateProgress(progressData.progress, progressData.maxProgress);
                progressStateData.Unfade();
                return;
            }
            
            foreach (var data in _popupQueue)
            {
                if (data is not ProgressData progress || data.title != progressData.title)
                {
                    continue;
                }

                if (progress.progress >= progress.maxProgress)
                {
                    continue;
                }

                progress.progress = progressData.progress;
                progress.maxProgress = progressData.maxProgress;
                        
                return;
            }
            
            _popupQueue.Enqueue(progressData);
        }
    }
}