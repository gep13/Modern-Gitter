﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Gitter.DataObjects.Concrete;
using Gitter.Services.Abstract;
using Gitter.ViewModel.Abstract;
using GitterSharp.Model;
using GitterSharp.Services;
using System.Threading.Tasks;
using Gitter.DataObjects.Abstract;
#if WINDOWS_PHONE_APP
using Windows.Media.SpeechRecognition;
#endif

namespace Gitter.ViewModel.Concrete
{
    public sealed class RoomViewModel : ViewModelBase, IRoomViewModel
    {
        #region Services

        private readonly IGitterApiService _gitterApiService;
        private readonly ILocalNotificationService _localNotificationService;
        private readonly IProgressIndicatorService _progressIndicatorService;
        private readonly IEventService _eventService;
        private readonly ITelemetryService _telemetryService;

        #endregion


        #region Fields

        private readonly IMainViewModel _mainViewModel;

        private IDisposable _streamingMessages;

        #endregion


        #region Properties

        public bool IsLoaded { get; set; }

        public Room Room { get; }

        public IncrementalLoadingCollection<IMessageViewModel> Messages { get; }

        private string _textMessage;
        public string TextMessage
        {
            get { return _textMessage; }
            set
            {
                _textMessage = value;
                RaisePropertyChanged();
                ((RelayCommand)SendMessageCommand).RaiseCanExecuteChanged();
            }
        }

        private bool _isSendingMessage;
        public bool IsSendingMessage
        {
            get { return _isSendingMessage; }
            set
            {
                _isSendingMessage = value;
                RaisePropertyChanged();
                ((RelayCommand)(SendMessageCommand)).RaiseCanExecuteChanged();
            }
        }

        private int _unreadMessagesCount;
        public int UnreadMessagesCount
        {
            get { return _unreadMessagesCount; }
            set
            {
                _unreadMessagesCount = value;
                RaisePropertyChanged();
            }
        }

        #endregion


        #region Commands

        public ICommand SendMessageCommand { get; private set; }
        public ICommand SendMessageWithParamCommand { get; private set; }
        public ICommand RemoveMessageCommand { get; private set; }
        public ICommand CopyMessageCommand { get; private set; }
        public ICommand RespondToCommand { get; private set; }
        public ICommand ViewProfileCommand { get; private set; }
        public ICommand TalkCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }

        #endregion


        #region Constructor

        public RoomViewModel(Room room,
            IGitterApiService gitterApiService,
            ILocalNotificationService localNotificationService,
            IProgressIndicatorService progressIndicatorService,
            IEventService eventService,
            ITelemetryService telemetryService,
            IMainViewModel mainViewModel)
        {
            // Properties
            Room = room;

            // View Models
            _mainViewModel = mainViewModel;

            // Commands
            SendMessageCommand = new RelayCommand(SendMessage, CanSendMessage);
            SendMessageWithParamCommand = new RelayCommand<bool>(SendMessageWithParam);
            RemoveMessageCommand = new RelayCommand<IMessageViewModel>(RemoveMessage, CanRemoveMessage);
            CopyMessageCommand = new RelayCommand<IMessageViewModel>(CopyMessage);
            RespondToCommand = new RelayCommand<User>(RespondTo);
            ViewProfileCommand = new RelayCommand<User>(ViewProfile);
            TalkCommand = new RelayCommand(Talk);
            RefreshCommand = new RelayCommand(Refresh);

            // Inject Services
            _gitterApiService = gitterApiService;
            _localNotificationService = localNotificationService;
            _progressIndicatorService = progressIndicatorService;
            _eventService = eventService;
            _telemetryService = telemetryService;


            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.

                var malditogeek = new User
                {
                    Id = "53307734c3599d1de448e192",
                    Username = "malditogeek",
                    DisplayName = "Mauro Pompilio",
                    Url = "/malditogeek",
                    SmallAvatarUrl = "https://avatars.githubusercontent.com/u/14751?",
                    MediumAvatarUrl = "https://avatars.githubusercontent.com/u/14751?"
                };

                var suprememoocow = new User
                {
                    Id = "53307831c3599d1de448e19a",
                    Username = "suprememoocow",
                    DisplayName = "Andrew Newdigate",
                    Url = "/suprememoocow,",
                    SmallAvatarUrl = "https://avatars.githubusercontent.com/u/594566?",
                    MediumAvatarUrl = "https://avatars.githubusercontent.com/u/594566?"
                };


                Messages = new MessagesIncrementalLoadingCollection("123456", gitterApiService, eventService, mainViewModel)
                {
                    new MessageViewModel(new Message
                    {
                        Id = "53316dc47bfc1a000000000f",
                        Text = "Hi @suprememoocow !",
                        Html =
                            "Hi <span data-link-type=\"mention\" data-screen-name=\"suprememoocow\" class=\"mention\">@suprememoocow</span> !",
                        SentDate = new DateTime(2014, 3, 25, 11, 51, 32),
                        EditedDate = null,
                        User = malditogeek,
                        UnreadByCurrent = false,
                        ReadCount = 0,
                        Urls = new List<MessageUrl>(),
                        Mentions = new List<Mention>
                        {
                            new Mention
                            {
                                ScreenName = "suprememoocow",
                                UserId = "53307831c3599d1de448e19a"
                            }
                        },
                        Issues = new List<Issue>(),
                        Version = 1
                    }),
                    new MessageViewModel(new Message
                    {
                        Id = "53316ec37bfc1a0000000011",
                        Text = "I've been working on #11, it'll be ready to ship soon",
                        Html =
                            "I&#39;ve been working on <span data-link-type=\"issue\" data-issue=\"11\" class=\"issue\">#11</span>, it&#39;ll be ready to ship soon",
                        SentDate = new DateTime(2014, 3, 25, 11, 55, 47),
                        EditedDate = null,
                        User = malditogeek,
                        UnreadByCurrent = false,
                        ReadCount = 0,
                        Urls = new List<MessageUrl>(),
                        Mentions = new List<Mention>(),
                        Issues = new List<Issue>
                        {
                            new Issue {Number = "11"}
                        },
                        Version = 1
                    }),
                    new MessageViewModel(new Message
                    {
                        Id = "53316ec37bfc1a0000000012",
                        Text = "This is a test message",
                        Html = "This is a test message",
                        SentDate = new DateTime(2014, 3, 25, 11, 55, 47),
                        EditedDate = null,
                        User = suprememoocow,
                        UnreadByCurrent = false,
                        ReadCount = 0,
                        Urls = new List<MessageUrl>(),
                        Mentions = new List<Mention>(),
                        Issues = new List<Issue>(),
                        Version = 1
                    }),
                    new MessageViewModel(new Message
                    {
                        Id = "53316ec37bfc1a0000000013",
                        Text = "Another long long ............... message",
                        Html = "Another long long ............... message",
                        SentDate = new DateTime(2014, 3, 25, 11, 55, 47),
                        EditedDate = null,
                        User = malditogeek,
                        UnreadByCurrent = false,
                        ReadCount = 0,
                        Urls = new List<MessageUrl>(),
                        Mentions = new List<Mention>(),
                        Issues = new List<Issue>(),
                        Version = 1
                    })
                };
            }
            else
            {
                // Code runs "for real"

                Messages = new MessagesIncrementalLoadingCollection(Room.Id, gitterApiService, eventService, mainViewModel);
                OpenRealtimeStream();
            }

            // Update count of unread messages
            UnreadMessagesCount = Room.UnreadItems;
        }

        #endregion


        #region Command Methods

        private bool CanSendMessage()
        {
            return !string.IsNullOrWhiteSpace(TextMessage) && !IsSendingMessage;
        }
        private async void SendMessage()
        {
            // Start async task
            await _progressIndicatorService.ShowAsync();
            IsSendingMessage = true;

            await _gitterApiService.SendMessageAsync(Room.Id, TextMessage);

            _telemetryService.TrackEvent("SendMessage",
                new Dictionary<string, string> { { "Room", Room.Name } },
                new Dictionary<string, double> { { "MessageLength", TextMessage.Length } });
            TextMessage = string.Empty;

            // End async task
            await _progressIndicatorService.HideAsync();
            IsSendingMessage = false;
        }

        private void SendMessageWithParam(bool enterKeyPressed)
        {
            if (enterKeyPressed && CanSendMessage())
                SendMessage();
        }

        private bool CanRemoveMessage(IMessageViewModel message)
        {
            if (message == null)
                return false;

            var currentDate = new DateTimeOffset(_mainViewModel.CurrentDateTime);
            double minutesLeft = currentDate.Subtract(message.SentDate).TotalMinutes;

            return (message.User.Id == _mainViewModel.CurrentUser.Id && minutesLeft < 10);
        }
        private async void RemoveMessage(IMessageViewModel message)
        {
            var updatedMessage = await _gitterApiService.UpdateMessageAsync(Room.Id, message.Id, string.Empty);
            message.UpdateMessage(updatedMessage.Text);
            Messages.Remove(message);

            double minutesLeft = _mainViewModel.CurrentDateTime.Subtract(message.SentDate).TotalMinutes;
            _telemetryService.TrackEvent("RemoveMessage",
                new Dictionary<string, string> { { "Room", Room.Name } },
                new Dictionary<string, double> { { "MinutesAgo", minutesLeft } });
        }

        private void CopyMessage(IMessageViewModel message)
        {
            TextMessage += $"{message.Text} ";

            _telemetryService.TrackEvent("CopyMessage",
                new Dictionary<string, string> { { "Room", Room.Name } });
        }

        private void RespondTo(User user)
        {
            TextMessage += $"@{user.Username} ";
        }

        private async void ViewProfile(User user)
        {
#if WINDOWS_PHONE_APP
            await Windows.System.Launcher.LaunchUriAsync(new Uri(user.GitHubUrl));
#endif
        }

        private async void Talk()
        {
#if WINDOWS_PHONE_APP
            // Create an instance of SpeechRecognizer
            var speechRecognizer = new SpeechRecognizer();

            // Compile the dictation grammar by default
            await speechRecognizer.CompileConstraintsAsync();

            // Start recognition
            var speechRecognitionResult = await speechRecognizer.RecognizeWithUIAsync();

            // Add text to message that will be sent
            TextMessage += speechRecognitionResult.Text;
#endif
        }

        private void Refresh()
        {
            Messages.Reset();

            _telemetryService.TrackEvent("RefreshRoom",
                new Dictionary<string, string> { { "Room", Room.Name } });
        }

        #endregion


        #region Methods

        public void OpenRealtimeStream()
        {
            // Use the stream API to add new messages when they comes
            _streamingMessages = _gitterApiService.GetRealtimeMessages(Room.Id)
                .Subscribe(async message => await NotifyNewMessageAsync(message));
        }

        public void CloseRealtimeStream()
        {
            _streamingMessages.Dispose();
        }

        private async Task NotifyNewMessageAsync(Message message)
        {
            // Do not add an existing messages to the chat
            if (Messages.Any(m => m.Id == message.Id))
                return;

            // Add message to the room
            _eventService.PushMessage
                .OnNext(new Tuple<string, IMessageViewModel>(Room.Id, new MessageViewModel(message)));

            // If the message was not read, update unread notifications when user is not reading 
            if (message.UnreadByCurrent)
            {
                // Add in-app notification (unread count), except for the current selected room
                if (_mainViewModel.SelectedRoom != this)
                {
                    var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                    await dispatcher.RunAsync(CoreDispatcherPriority.High, () => UnreadMessagesCount++);
                }

                // Send notification (new message)
                if (!Room.DisabledNotifications && _mainViewModel.CurrentUser.Id != message.User.Id)
                {
                    string id = $"{Room.Name}_message_{Room.Id}";
                    _localNotificationService.SendNotification(Room.Name, message.Text, id, Room.Name);
                }
            }
        }

        #endregion
    }
}
