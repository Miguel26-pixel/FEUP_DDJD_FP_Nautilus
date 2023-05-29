using System;
using System.Collections.Generic;
using UI.Inventory;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public record PopupData
    {
        public Sprite icon;
        public readonly IconRepository.IconType iconType;
        public readonly string title;

        public PopupData(string title, Sprite icon)
        {
            this.title = title;
            this.icon = icon;
        }

        public PopupData(string title, IconRepository.IconType iconType)
        {
            this.title = title;
            icon = null;
            this.iconType = iconType;
        }
    }

    public record ProgressData : PopupData
    {
        public float maxProgress;
        public float progress;

        public ProgressData(string title, Sprite icon, float progress, float maxProgress) : base(title, icon)
        {
            this.maxProgress = maxProgress;
            this.progress = progress;
        }
    }

    public abstract class StateData
    {
        private readonly float _popupDuration;
        private readonly float _popupFadeDuration;
        public readonly VisualElement popup;

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
                    if (_time >= _popupDuration)
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
        private readonly ProgressBar _progressBar;
        public readonly ProgressData progressData;

        public ProgressStateData(ProgressData progressData, VisualElement progress, float popupDuration,
            float popupFadeDuration) :
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

        public bool Completed => _progressBar.value >= _progressBar.highValue;

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

        private IconRepository _iconRepository;

        private readonly Queue<PopupData> _popupQueue = new();

        private readonly LinkedList<PopupStateData> _popupStateData = new();
        private readonly LinkedList<ProgressStateData> _progressStateData = new();
        private VisualElement _popupContainer;

        private int _popupCount;

        private VisualElement _root;


        private void Start()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;

            _popupContainer = _root.Q<VisualElement>("PopupContainer");
            _iconRepository = GameObject.Find("IconRepository").GetComponent<IconRepository>();
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

            foreach (PopupStateData popupStateData in _popupStateData)
            {
                PopupState state = popupStateData.Update(deltaTime);

                if (state == PopupState.Closed)
                {
                    _popupCount--;
                    _popupStateData.Remove(popupStateData);
                    break;
                }
            }

            foreach (ProgressStateData progressStateData in _progressStateData)
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
                ProgressStateData progressStateData = new(progressData, progressVisualTreeAsset.CloneTree(),
                    popupDuration, popupFadeDuration);

                _progressStateData.AddLast(progressStateData);
                _popupContainer.Add(progressStateData.popup);
            }
            else
            {
                PopupStateData popupStateData = new(popupData, popupVisualTreeAsset.CloneTree(), popupDuration,
                    popupFadeDuration);

                _popupStateData.AddLast(popupStateData);
                _popupContainer.Add(popupStateData.popup);
            }

            _popupCount++;
        }

        public void QueuePopup(PopupData popupData)
        {
            foreach (PopupStateData popupStateData in _popupStateData)
            {
                if (popupStateData.popupData.title != popupData.title)
                {
                    continue;
                }

                popupStateData.Unfade();
                return;
            }

            foreach (PopupData data in _popupQueue)
            {
                if (data is not ProgressData && data.title == popupData.title)
                {
                    return;
                }
            }
            
            if (popupData.icon == null)
            {
                popupData.icon = _iconRepository.GetIcon(popupData.iconType);
            }

            _popupQueue.Enqueue(popupData);
        }

        public void ShowProgress(ProgressData progressData)
        {
            foreach (ProgressStateData progressStateData in _progressStateData)
            {
                if (progressStateData.progressData.title != progressData.title || progressStateData.Completed)
                {
                    continue;
                }

                progressStateData.UpdateProgress(progressData.progress, progressData.maxProgress);
                progressStateData.Unfade();
                return;
            }

            foreach (PopupData data in _popupQueue)
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