﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace FitLog.Controls
{
    /// <summary>
    /// Логика взаимодействия для CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {

        #region Fields
        private static readonly Dictionary<MessageWindowImage, Style> _images = new Dictionary<MessageWindowImage, Style>(capacity: 3)
        {
            [MessageWindowImage.Error] = (Style)Application.Current.Resources["ErrorMessage"],
            [MessageWindowImage.Warning] = (Style)Application.Current.Resources["AttentionMessage"],
            [MessageWindowImage.Information] = (Style)Application.Current.Resources["InformationMessage"],
        };

        private readonly Dictionary<MessageWindowButton, Action> _buttons;
        #endregion Fields

        #region Constructors
        public CustomMessageBox()
        {
            InitializeComponent();
            _buttons = new Dictionary<MessageWindowButton, Action>(capacity: 5)
            {
                [MessageWindowButton.Ok] = ShowOkButton,
                [MessageWindowButton.OkCancel] = ShowOkCancelButtons,
                [MessageWindowButton.YesNo] = ShowYesNoButtons,
                [MessageWindowButton.YesNoCancel] = ShowYesNoCancelButtons
            };
        }
        #endregion Constructors

        #region Properties
        public new MessageWindowResult DialogResult { get; set; }
        #endregion Properties

        #region Enums
        public enum MessageWindowResult
        {
            Ok,
            Cancel,
            Yes,
            No,
            Close
        }

        public enum MessageWindowImage
        {
            Error,
            Warning,
            Information
        }

        public enum MessageWindowButton
        {
            None,
            Ok,
            OkCancel,
            YesNo,
            YesNoCancel
        }
        #endregion Enums

        #region Methods
        private void SetImage(Style imageStyle)
            => Img.Style = imageStyle;

        private void SetText(string text)
            => Text.Text = text;

        private void SetButton(MessageWindowButton buttons)
        {
            if (buttons.Equals(MessageWindowButton.None))
                return;

            _buttons[buttons]();
        }
        private void ShowOkButton()
        {
            OkButton.Visibility = Visibility.Visible;
            OkButton.IsDefault = true;
        }

        private void ShowCancelButton()
            => CancelButton.Visibility = Visibility.Visible;

        private void ShowOkCancelButtons()
        {
            ShowOkButton();
            ShowCancelButton();
        }

        private void ShowYesButton()
            => YesButton.Visibility = Visibility.Visible;

        private void ShowNoButton()
            => NoButton.Visibility = Visibility.Visible;

        private void ShowYesNoButtons()
        {
            ShowYesButton();
            ShowNoButton();
            YesButton.IsDefault = true;
        }

        private void ShowYesNoCancelButtons()
        {
            ShowYesNoButtons();
            ShowCancelButton();
        }

        private static CustomMessageBox CreateDefaultMessageWindow(string text, string windowTitle, MessageWindowImage image, MessageWindowButton buttons)
        {
            CustomMessageBox window = new CustomMessageBox();
            window.SetImage(imageStyle: _images[image]);
            window.SetButton(buttons: buttons);
            window.Name = windowTitle;
            window.Owner = Application.Current.MainWindow;
            window.ShowInTaskbar = false;
            window.SetText(text: text);
            return window;
        }

        public static MessageWindowResult Show(string text, string windowTitle, MessageWindowImage image, MessageWindowButton buttons)
        {
            CustomMessageBox window = CreateDefaultMessageWindow(text: text, windowTitle: windowTitle, image: image, buttons: buttons);
            window.ShowDialog();
            return window.DialogResult;
        }

        public static MessageWindowResult ShowError(string text)
            => Show(image: MessageWindowImage.Error, windowTitle: "Ошибка", text: text, buttons: MessageWindowButton.OkCancel);

        public static MessageWindowResult ShowWarning(string text)
            => Show(image: MessageWindowImage.Warning, windowTitle: "Предупреждение", text: text, buttons: MessageWindowButton.OkCancel);

        public static MessageWindowResult ShowInformation(string text)
            => Show(image: MessageWindowImage.Information, windowTitle: "Сообщение", text: text, buttons: MessageWindowButton.Ok);

        public static void ShowMessage(string text)
        {
            CustomMessageBox window = CreateDefaultMessageWindow(image: MessageWindowImage.Information, windowTitle: "Сообщение", text: text, buttons: MessageWindowButton.None);
            Grid.SetRowSpan(element: window.Img, value: 2);
            Grid.SetRowSpan(element: window.Text, value: 2);
            window.ShowDialog();
        }

        private void ButtonClick(MessageWindowResult result)
        {
            Close();
            DialogResult = result;
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
            => ButtonClick(result: MessageWindowResult.Cancel);

        private void OnNoButtonClick(object sender, RoutedEventArgs e)
            => ButtonClick(result: MessageWindowResult.No);

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
            => ButtonClick(result: MessageWindowResult.Ok);

        private void OnYesButtonClick(object sender, RoutedEventArgs e)
            => ButtonClick(result: MessageWindowResult.Yes);

        private void OnExitButtonClick(object sender, RoutedEventArgs e)
            => ButtonClick(result: MessageWindowResult.Close);
        #endregion Methods
    }

}

