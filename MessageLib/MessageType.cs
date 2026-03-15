namespace MessageLib
{
    public enum MessageType
    {
        Unknown = 0,

#region FromUserToUser

        Text = 10,//текстовое сообщение от юзера к юзеру
        Picture = 11,//изображение от юзера к юзеру
        Voice = 12,//голосовое от юзера к юзеру
        AddToGroupRequest = 13,//запрос на вступление в группу (user->user)
#endregion

#region FromUserToSystem
        
        AddUserToGroupResponse = 50,//уведомление о добавлении нового юзера в групповой чат

        RemoveUserFromGroupRequest = 51,//юзера выкидывает сам админ группы (user->system)
        RemoveUserFromGroupResponce = 52,

        CreateGroupRequest = 53,//создание новой группы (user->ыныеуь)
        CreateGroupResponce = 54,

        DeleteGroupRequest = 55,//удаление группы и ее чатов самим админом группы (user->system)
        DeleteGroupResponce = 56,

        DeleteSelfAccountRequest=57,//удаляет аккаунт на сервере
        DeleteSelfAccountResponce=58,

        UserAddedToGroupResponse=59,//для уведомления юзера в какую группу ЕГО добавили
#endregion

        SystemNotify=60//для оповещения всех юзеров 
    }
}
