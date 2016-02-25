using System.IO;
using System.Runtime.Serialization.Json;

namespace Sulakore.Habbo.Headers
{
    public class Incoming
    {
        private static readonly DataContractJsonSerializer _serializer;

        public static Incoming Global { get; }

        #region Packet Properties/Names
        public int AchievementProgressed { get; set; }
        public int AchievementScore { get; set; }
        public int AchievementUnlocked { get; set; }
        public int Achievements { get; set; }
        public int Action { get; set; }
        public int ActivityPoints { get; set; }
        public int AddExperiencePoints { get; set; }
        public int AuthenticationOK { get; set; }
        public int AvailabilityStatus { get; set; }
        public int AvatarEffectActivated { get; set; }
        public int AvatarEffectAdded { get; set; }
        public int AvatarEffectExpired { get; set; }
        public int AvatarEffects { get; set; }
        public int AvatarEffect { get; set; }
        public int BCBorrowedItems { get; set; }
        public int BadgeDefinitions { get; set; }
        public int BadgeEditorParts { get; set; }
        public int Badges { get; set; }
        public int BotInventory { get; set; }
        public int BroadcastMessageAlert { get; set; }
        public int BuddyList { get; set; }
        public int BuddyRequests { get; set; }
        public int BuildersClubMembership { get; set; }
        public int CampaignCalendarData { get; set; }
        public int Campaign { get; set; }
        public int CanCreateRoom { get; set; }
        public int CantConnect { get; set; }
        public int CarryObject { get; set; }
        public int CatalogIndex { get; set; }
        public int CatalogItemDiscount { get; set; }
        public int CatalogOffer { get; set; }
        public int CatalogPage { get; set; }
        public int CatalogUpdated { get; set; }
        public int CfhTopicsInit { get; set; }
        public int Chat { get; set; }
        public int CheckGnomeName { get; set; }
        public int CheckPetName { get; set; }
        public int CloseConnection { get; set; }
        public int ClubGifts { get; set; }
        public int CommunityGoalHallOfFame { get; set; }
        public int ConcurrentUsersGoalProgress { get; set; }
        public int CreditBalance { get; set; }
        public int Dance { get; set; }
        public int Doorbell { get; set; }
        public int EnforceCategoryUpdate { get; set; }
        public int Favourites { get; set; }
        public int FigureSetIds { get; set; }
        public int FindFriendsProcessResult { get; set; }
        public int FlatAccessDenied { get; set; }
        public int FlatAccessible { get; set; }
        public int FlatControllerAdded { get; set; }
        public int FlatControllerRemoved { get; set; }
        public int FlatCreated { get; set; }
        public int FloodControl { get; set; }
        public int FloorHeightMap { get; set; }
        public int FloorPlanFloorMap { get; set; }
        public int FloorPlanSendDoor { get; set; }
        public int FollowFriendFailed { get; set; }
        public int ForumData { get; set; }
        public int ForumsListData { get; set; }
        public int FriendListUpdate { get; set; }
        public int FriendNotification { get; set; }
        public int FurniListAdd { get; set; }
        public int FurniListNotification { get; set; }
        public int FurniListRemove { get; set; }
        public int FurniListUpdate { get; set; }
        public int FurniList { get; set; }
        public int FurnitureAliases { get; set; }
        public int Game1WeeklyLeaderboard { get; set; }
        public int Game2WeeklyLeaderboard { get; set; }
        public int Game3WeeklyLeaderboard { get; set; }
        public int GameAccountStatus { get; set; }
        public int GameAchievementList { get; set; }
        public int GameList { get; set; }
        public int GenericError { get; set; }
        public int GetGuestRoomResult { get; set; }
        public int GetRelationships { get; set; }
        public int GetRoomBannedUsers { get; set; }
        public int GetRoomFilterList { get; set; }
        public int GetYouTubePlaylist { get; set; }
        public int GetYouTubeVideo { get; set; }
        public int GiftWrappingConfiguration { get; set; }
        public int GiftWrappingError { get; set; }
        public int GnomeBox { get; set; }
        public int GroupCreationWindow { get; set; }
        public int GroupFurniConfig { get; set; }
        public int GroupFurniSettings { get; set; }
        public int GroupInfo { get; set; }
        public int GroupMemberUpdated { get; set; }
        public int GroupMembershipRequested { get; set; }
        public int GroupMembers { get; set; }
        public int GuestRoomSearchResult { get; set; }
        public int HabboActivityPointNotification { get; set; }
        public int HabboGroupBadges { get; set; }
        public int HabboSearchResult { get; set; }
        public int HabboUserBadges { get; set; }
        public int HeightMap { get; set; }
        public int HelperTool { get; set; }
        public int HideWiredConfig { get; set; }
        public int IgnoreStatus { get; set; }
        public int InitCrypto { get; set; }
        public int InstantMessageError { get; set; }
        public int ItemAdd { get; set; }
        public int ItemRemove { get; set; }
        public int ItemUpdate { get; set; }
        public int Items { get; set; }
        public int JoinQueue { get; set; }
        public int LatencyResponse { get; set; }
        public int LoadGame { get; set; }
        public int LoveLockDialogueClose { get; set; }
        public int LoveLockDialogueSetLocked { get; set; }
        public int LoveLockDialogue { get; set; }
        public int MOTDNotification { get; set; }
        public int MaintenanceStatus { get; set; }
        public int ManageGroup { get; set; }
        public int MarketPlaceOffers { get; set; }
        public int MarketPlaceOwnOffers { get; set; }
        public int MarketplaceCanMakeOfferResult { get; set; }
        public int MarketplaceCancelOfferResult { get; set; }
        public int MarketplaceConfiguration { get; set; }
        public int MarketplaceItemStats { get; set; }
        public int MarketplaceMakeOfferResult { get; set; }
        public int MessengerError { get; set; }
        public int MessengerInit { get; set; }
        public int ModeratorInit { get; set; }
        public int ModeratorRoomChatlog { get; set; }
        public int ModeratorRoomInfo { get; set; }
        public int ModeratorSupportTicketResponse { get; set; }
        public int ModeratorSupportTicket { get; set; }
        public int ModeratorTicketChatlog { get; set; }
        public int ModeratorUserChatlog { get; set; }
        public int ModeratorUserInfo { get; set; }
        public int ModeratorUserRoomVisits { get; set; }
        public int MoodlightConfig { get; set; }
        public int Muted { get; set; }
        public int NameChangeUpdate { get; set; }
        public int NavigatorCollapsedCategories { get; set; }
        public int NavigatorFlatCats { get; set; }
        public int NavigatorLiftedRooms { get; set; }
        public int NavigatorMetaDataParser { get; set; }
        public int NavigatorPreferences { get; set; }
        public int NavigatorSearchResultSet { get; set; }
        public int NavigatorSettings { get; set; }
        public int NewBuddyRequest { get; set; }
        public int NewConsoleMessage { get; set; }
        public int NewGroupInfo { get; set; }
        public int NewUserExperienceGiftOffer { get; set; }
        public int ObjectAdd { get; set; }
        public int ObjectRemove { get; set; }
        public int ObjectUpdate { get; set; }
        public int Objects { get; set; }
        public int OpenBotAction { get; set; }
        public int OpenConnection { get; set; }
        public int OpenGift { get; set; }
        public int OpenHelpTool { get; set; }
        public int PetBreeding { get; set; }
        public int PetHorseFigureInformation { get; set; }
        public int PetInformation { get; set; }
        public int PetInventory { get; set; }
        public int PetTrainingPanel { get; set; }
        public int PlayableGames { get; set; }
        public int Pong { get; set; }
        public int PopularRoomTagsResult { get; set; }
        public int PostUpdated { get; set; }
        public int PresentDeliverError { get; set; }
        public int ProfileInformation { get; set; }
        public int PromoArticles { get; set; }
        public int PromotableRooms { get; set; }
        public int PurchaseError { get; set; }
        public int PurchaseOK { get; set; }
        public int QuestAborted { get; set; }
        public int QuestCompleted { get; set; }
        public int QuestList { get; set; }
        public int QuestStarted { get; set; }
        public int QuestionParser { get; set; }
        public int RecyclerRewards { get; set; }
        public int RefreshFavouriteGroup { get; set; }
        public int RentableSpacesError { get; set; }
        public int RentableSpace { get; set; }
        public int RespectNotification { get; set; }
        public int RespectPetNotification { get; set; }
        public int RoomEntryInfo { get; set; }
        public int RoomErrorNotif { get; set; }
        public int RoomEvent { get; set; }
        public int RoomForward { get; set; }
        public int RoomInfoUpdated { get; set; }
        public int RoomInvite { get; set; }
        public int RoomMuteSettings { get; set; }
        public int RoomNotification { get; set; }
        public int RoomProperty { get; set; }
        public int RoomRating { get; set; }
        public int RoomReady { get; set; }
        public int RoomRightsList { get; set; }
        public int RoomSettingsData { get; set; }
        public int RoomSettingsSaved { get; set; }
        public int RoomVisualizationSettings { get; set; }
        public int SanctionStatus { get; set; }
        public int ScrSendUserInfo { get; set; }
        public int SecretKey { get; set; }
        public int SellablePetBreeds { get; set; }
        public int SendBullyReport { get; set; }
        public int SendGameInvitation { get; set; }
        public int SetGroupId { get; set; }
        public int SetUniqueId { get; set; }
        public int Shout { get; set; }
        public int Sleep { get; set; }
        public int SlideObjectBundle { get; set; }
        public int SoundSettings { get; set; }
        public int StickyNote { get; set; }
        public int SubmitBullyReport { get; set; }
        public int TalentLevelUp { get; set; }
        public int TalentTrackLevel { get; set; }
        public int TalentTrack { get; set; }
        public int ThreadCreated { get; set; }
        public int ThreadData { get; set; }
        public int ThreadReply { get; set; }
        public int ThreadUpdated { get; set; }
        public int ThreadsListData { get; set; }
        public int TradingAccept { get; set; }
        public int TradingClosed { get; set; }
        public int TradingComplete { get; set; }
        public int TradingConfirmed { get; set; }
        public int TradingError { get; set; }
        public int TradingFinish { get; set; }
        public int TradingStart { get; set; }
        public int TradingUpdate { get; set; }
        public int TraxSongInfo { get; set; }
        public int UnbanUserFromRoom { get; set; }
        public int UnknownCalendar { get; set; }
        public int UnknownGroup { get; set; }
        public int UpdateFavouriteGroup { get; set; }
        public int UpdateFavouriteRoom { get; set; }
        public int UpdateFreezeLives { get; set; }
        public int UpdateMagicTile { get; set; }
        public int UpdateUsername { get; set; }
        public int UserChange { get; set; }
        public int UserFlatCats { get; set; }
        public int UserNameChange { get; set; }
        public int UserObject { get; set; }
        public int UserPerks { get; set; }
        public int UserRemove { get; set; }
        public int UserRights { get; set; }
        public int UserTags { get; set; }
        public int UserTyping { get; set; }
        public int UserUpdate { get; set; }
        public int Users { get; set; }
        public int VideoOffersRewards { get; set; }
        public int VoucherRedeemError { get; set; }
        public int VoucherRedeemOk { get; set; }
        public int Wardrobe { get; set; }
        public int Whisper { get; set; }
        public int WiredConditionConfig { get; set; }
        public int WiredEffectConfig { get; set; }
        public int WiredTriggerConfig { get; set; }
        public int YouAreController { get; set; }
        public int YouAreNotController { get; set; }
        public int YouAreOwner { get; set; }
        #endregion

        static Incoming()
        {
            _serializer = new DataContractJsonSerializer(typeof(Incoming));

            Global = new Incoming();
        }

        public void Save(string path)
        {
            using (var fileStream = File.Open(path, FileMode.Create))
                _serializer.WriteObject(fileStream, this);
        }
        public static Incoming Load(string path)
        {
            using (var fileStream = File.Open(path, FileMode.Open))
                return (Incoming)_serializer.ReadObject(fileStream);
        }
    }
}