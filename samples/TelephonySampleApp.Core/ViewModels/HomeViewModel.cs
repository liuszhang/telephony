﻿using System.Runtime.Serialization;
using Telephony;

namespace TelephonySampleApp.Core.ViewModels
{
    [DataContract]
    public class HomeViewModel : ReactiveObject, IHomeViewModel, IEnableLogger
    {
        [IgnoreDataMember] private readonly ITelephonyService TelephonyService;
        [IgnoreDataMember] private string _recipient;

        public HomeViewModel(ITelephonyService telephonyService = null, IScreen hostScreen = null)
        {
            TelephonyService = telephonyService ?? Locator.Current.GetService<ITelephonyService>();

            HostScreen = hostScreen ?? Locator.Current.GetService<IScreen>();

            var canComposeSMS = this.WhenAny(x => x.Recipient, x => !string.IsNullOrWhiteSpace(x.Value));
            ComposeSMS = ReactiveCommand.CreateAsyncTask(canComposeSMS,
                async _ => { await TelephonyService.ComposeSMS(Recipient); });
            ComposeSMS.ThrownExceptions.Subscribe(
                ex => UserError.Throw("Does this device have the capability to send SMS?", ex));

            var canComposeEmail = this.WhenAny(x => x.Recipient, x => !string.IsNullOrWhiteSpace(x.Value));
            ComposeEmail = ReactiveCommand.CreateAsyncTask(canComposeEmail, async _ =>
            {
                var email = new Email(Recipient);

                await TelephonyService.ComposeEmail(email);
            });
            ComposeEmail.ThrownExceptions.Subscribe(
                ex => UserError.Throw("The recipient is potentially not a well formed email address.", ex));

            var canMakePhoneCall = this.WhenAny(x => x.Recipient, x => !string.IsNullOrWhiteSpace(x.Value));
            MakePhoneCall = ReactiveCommand.CreateAsyncTask(canMakePhoneCall,
                async _ => { await TelephonyService.MakePhoneCall(Recipient); });
            MakePhoneCall.ThrownExceptions.Subscribe(
                ex => UserError.Throw("Does this device have the capability to make phone calls?", ex));

            var canMakeVideoCall = this.WhenAny(x => x.Recipient, x => !string.IsNullOrWhiteSpace(x.Value));
            MakeVideoCall = ReactiveCommand.CreateAsyncTask(canMakeVideoCall,
                async _ => { await TelephonyService.MakeVideoCall(Recipient); });
            MakeVideoCall.ThrownExceptions.Subscribe(
                ex => UserError.Throw("Does this device have the capability to make video calls?", ex));
        }

        [IgnoreDataMember]
        public IScreen HostScreen { get; protected set; }

        [IgnoreDataMember]
        public string UrlPathSegment
        {
            get { return "Telephony"; }
        }

        [IgnoreDataMember]
        public ReactiveCommand<Unit> ComposeEmail { get; set; }

        [IgnoreDataMember]
        public ReactiveCommand<Unit> ComposeSMS { get; set; }

        [IgnoreDataMember]
        public ReactiveCommand<Unit> MakePhoneCall { get; set; }

        [IgnoreDataMember]
        public ReactiveCommand<Unit> MakeVideoCall { get; set; }

        [DataMember]
        public string Recipient
        {
            get { return _recipient; }
            set { this.RaiseAndSetIfChanged(ref _recipient, value); }
        }

        private static bool IsAValidPhoneNumber(string s)
        {
            int result;
            var phoneNumber = s.Replace(" ", "")
                .Replace("-", "")
                .Replace("+", "")
                .Replace("(", "")
                .Replace(")", "");

            return int.TryParse(phoneNumber, out result);
        }
    }
}