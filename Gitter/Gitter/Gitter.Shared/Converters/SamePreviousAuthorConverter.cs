﻿using System;
using Windows.UI.Xaml.Data;
using Gitter.Model;
using Gitter.ViewModel;

namespace Gitter.Converters
{
    public class SamePreviousAuthorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var message = value as Message;

            if (message == null)
                throw new ArgumentNullException("value");

            var currentIndex = ViewModelLocator.Main.SelectedRoom.Messages.IndexOf(message);

            if (currentIndex == 0)
                return false;

            var previousMessage = ViewModelLocator.Main.SelectedRoom.Messages[currentIndex - 1];

            return message.User.Id == previousMessage.User.Id;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}