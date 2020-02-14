using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Native.Sdk.Cqp.EventArgs;
using System.Windows;
using PropertyChanged;
using System.Windows.Data;
using GalaSoft.MvvmLight.Command;
using Native.Sdk.Cqp;
using UI.Model;

namespace UI.Data
{
    [AddINotifyPropertyChangedInterface]
    public class MainData
    {
        public CQApi Api { get; set; }

        public CQLog Log { get; set; }

        public string UISettingPath { get; set; }

        public string Title { get; set; }

        public object SyncLock { get; set; } = new object();

        public ObservableCollection<Message> GroupMessages { get; set; } = new ObservableCollection<Message>();

        public ObservableCollection<Group> Groups { get; set; } = new ObservableCollection<Group>();

        public string ReadyToSend { get; set; }

        public Group SelectedGroup { get; set; }

        public RelayCommand SendCommand => new RelayCommand(OnSend);

        public void OnSend()
        {
            if (string.IsNullOrEmpty(ReadyToSend)) { MessageBox.Show("請先輸入信息"); return; }
            Api.SendGroupMessage(SelectedGroup.GroupId, ReadyToSend);
            BindingOperations.EnableCollectionSynchronization(GroupMessages, SyncLock);
            GroupMessages.Add(new Message()
            {
                TargetSide = false,
                DisplayName = Api.GetGroupMemberInfo(SelectedGroup.GroupId, Api.GetLoginQQ().Id).Nick,
                GroupName = SelectedGroup.GroupName,
                Content = ReadyToSend,
                GroupId = SelectedGroup.GroupId,
                Qq = Api.GetLoginQQ().Id
            });
            ReadyToSend = string.Empty;
        }
    }

}
