using System.IO;
using System.Runtime.Serialization.Json;

namespace Sulakore.Habbo.Headers
{
    public class Outgoing
    {
        private static readonly DataContractJsonSerializer _serializer;

        public static Outgoing Global { get; }

        #region Packet Properties/Names
        public int AcceptBuddy { get; set; }
        public int AcceptGroupMembership { get; set; }
        public int Action { get; set; }
        public int AddFavouriteRoom { get; set; }
        public int AddStickyNote { get; set; }
        public int ApplyDecoration { get; set; }
        public int ApplyHorseEffect { get; set; }
        public int ApplySign { get; set; }
        public int AssignRights { get; set; }
        public int AvatarEffectActivated { get; set; }
        public int AvatarEffectSelected { get; set; }
        public int BanUser { get; set; }
        public int BuyOffer { get; set; }
        public int CanCreateRoom { get; set; }
        public int CancelOffer { get; set; }
        public int CancelQuest { get; set; }
        public int CancelTyping { get; set; }
        public int ChangeMotto { get; set; }
        public int ChangeName { get; set; }
        public int Chat { get; set; }
        public int CheckGnomeName { get; set; }
        public int CheckPetName { get; set; }
        public int CheckValidName { get; set; }
        public int ClientVariables { get; set; }
        public int CloseTicketMesageEvent { get; set; }
        public int ConfirmLoveLock { get; set; }
        public int CraftSecret { get; set; }
        public int CreateFlat { get; set; }
        public int CreditFurniRedeem { get; set; }
        public int Dance { get; set; }
        public int DeclineBuddy { get; set; }
        public int DeclineGroupMembership { get; set; }
        public int DeleteFavouriteRoom { get; set; }
        public int DeleteGroup { get; set; }
        public int DeleteGroupPost { get; set; }
        public int DeleteGroupThread { get; set; }
        public int DeleteRoom { get; set; }
        public int DeleteStickyNote { get; set; }
        public int DiceOff { get; set; }
        public int Disconnection { get; set; }
        public int DropHandItem { get; set; }
        public int EditRoomPromotion { get; set; }
        public int EventTracker { get; set; }
        public int FindNewFriends { get; set; }
        public int FindRandomFriendingRoom { get; set; }
        public int FloorPlanEditorRoomProperties { get; set; }
        public int FollowFriend { get; set; }
        public int ForceOpenCalendarBox { get; set; }
        public int FriendListUpdate { get; set; }
        public int Game2GetWeeklyLeaderboard { get; set; }
        public int GenerateSecretKey { get; set; }
        public int GetAchievements { get; set; }
        public int GetBadgeEditorParts { get; set; }
        public int GetBadges { get; set; }
        public int GetBotInventory { get; set; }
        public int GetBuddyRequests { get; set; }
        public int GetCatalogIndex { get; set; }
        public int GetCatalogMode { get; set; }
        public int GetCatalogOffer { get; set; }
        public int GetCatalogPage { get; set; }
        public int GetCatalogRoomPromotion { get; set; }
        public int GetClientVersion { get; } = 4000;
        public int GetClubGifts { get; set; }
        public int GetCraftingRecipesAvailable { get; set; }
        public int GetCreditsInfo { get; set; }
        public int GetCurrentQuest { get; set; }
        public int GetDailyQuest { get; set; }
        public int GetEventCategories { get; set; }
        public int GetForumStats { get; set; }
        public int GetForumUserProfile { get; set; }
        public int GetForumsListData { get; set; }
        public int GetFurnitureAliases { get; set; }
        public int GetGameListing { get; set; }
        public int GetGiftWrappingConfiguration { get; set; }
        public int GetGroupCreationWindow { get; set; }
        public int GetGroupFurniConfig { get; set; }
        public int GetGroupFurniSettings { get; set; }
        public int GetGroupInfo { get; set; }
        public int GetGroupMembers { get; set; }
        public int GetGuestRoom { get; set; }
        public int GetHabboClubWindow { get; set; }
        public int GetHabboGroupBadges { get; set; }
        public int GetMarketplaceCanMakeOffer { get; set; }
        public int GetMarketplaceConfiguration { get; set; }
        public int GetMarketplaceItemStats { get; set; }
        public int GetModeratorRoomChatlog { get; set; }
        public int GetModeratorRoomInfo { get; set; }
        public int GetModeratorTicketChatlogs { get; set; }
        public int GetModeratorUserChatlog { get; set; }
        public int GetModeratorUserInfo { get; set; }
        public int GetModeratorUserRoomVisits { get; set; }
        public int GetMoodlightConfig { get; set; }
        public int GetOffers { get; set; }
        public int GetOwnOffers { get; set; }
        public int GetPetInformation { get; set; }
        public int GetPetInventory { get; set; }
        public int GetPetTrainingPanel { get; set; }
        public int GetPlayableGames { get; set; }
        public int GetPromoArticles { get; set; }
        public int GetPromotableRooms { get; set; }
        public int GetQuestList { get; set; }
        public int GetRecipeConfig { get; set; }
        public int GetRecyclerRewards { get; set; }
        public int GetRelationships { get; set; }
        public int GetRentableSpace { get; set; }
        public int GetRoomBannedUsers { get; set; }
        public int GetRoomEntryData { get; set; }
        public int GetRoomFilterList { get; set; }
        public int GetRoomRights { get; set; }
        public int GetRoomSettings { get; set; }
        public int GetSanctionStatus { get; set; }
        public int GetSelectedBadges { get; set; }
        public int GetSellablePetBreeds { get; set; }
        public int GetSongInfo { get; set; }
        public int GetStickyNote { get; set; }
        public int GetTalentTrack { get; set; }
        public int GetThreadData { get; set; }
        public int GetThreadsListData { get; set; }
        public int GetUserFlatCats { get; set; }
        public int GetUserTags { get; set; }
        public int GetWardrobe { get; set; }
        public int GetYouTubeTelevision { get; set; }
        public int GiveAdminRights { get; set; }
        public int GiveHandItem { get; set; }
        public int GiveRoomScore { get; set; }
        public int GoToFlat { get; set; }
        public int GoToHotelView { get; set; }
        public int HabboSearch { get; set; }
        public int IgnoreUser { get; set; }
        public int InfoRetrieve { get; set; }
        public int InitCrypto { get; set; }
        public int InitTrade { get; set; }
        public int InitializeFloorPlanSession { get; set; }
        public int InitializeGameCenter { get; set; }
        public int InitializeNewNavigator { get; set; }
        public int JoinGroup { get; set; }
        public int JoinPlayerQueue { get; set; }
        public int KickUser { get; set; }
        public int LatencyTest { get; set; }
        public int LetUserIn { get; set; }
        public int LookTo { get; set; }
        public int MakeOffer { get; set; }
        public int ManageGroup { get; set; }
        public int MemoryPerformance { get; set; }
        public int MessengerInit { get; set; }
        public int ModerateRoom { get; set; }
        public int ModerationBan { get; set; }
        public int ModerationCaution { get; set; }
        public int ModerationKick { get; set; }
        public int ModerationMsg { get; set; }
        public int ModerationMute { get; set; }
        public int ModerationTradeLock { get; set; }
        public int ModeratorAction { get; set; }
        public int ModifyRoomFilterList { get; set; }
        public int ModifyWhoCanRideHorse { get; set; }
        public int MoodlightUpdate { get; set; }
        public int MoveAvatar { get; set; }
        public int MoveObject { get; set; }
        public int MoveWallItem { get; set; }
        public int MuteUser { get; set; }
        public int NewNavigatorSearch { get; set; }
        public int OnBullyClick { get; set; }
        public int OpenBotAction { get; set; }
        public int OpenCalendarBox { get; set; }
        public int OpenFlatConnection { get; set; }
        public int OpenGift { get; set; }
        public int OpenHelpTool { get; set; }
        public int OpenPlayerProfile { get; set; }
        public int PickTicket { get; set; }
        public int PickUpBot { get; set; }
        public int PickUpPet { get; set; }
        public int PickupObject { get; set; }
        public int Ping { get; set; }
        public int PlaceBot { get; set; }
        public int PlaceObject { get; set; }
        public int PlacePet { get; set; }
        public int PostGroupContent { get; set; }
        public int PurchaseFromCatalog { get; set; }
        public int PurchaseFromCatalogAsGift { get; set; }
        public int PurchaseGroup { get; set; }
        public int PurchaseRoomPromotion { get; set; }
        public int RedeemOfferCredits { get; set; }
        public int RedeemVoucher { get; set; }
        public int RefreshCampaign { get; set; }
        public int ReleaseTicket { get; set; }
        public int RemoveAllRights { get; set; }
        public int RemoveBuddy { get; set; }
        public int RemoveGroupFavourite { get; set; }
        public int RemoveGroupMember { get; set; }
        public int RemoveMyRights { get; set; }
        public int RemoveRights { get; set; }
        public int RemoveSaddleFromHorse { get; set; }
        public int RequestBuddy { get; set; }
        public int RequestFurniInventory { get; set; }
        public int RespectPet { get; set; }
        public int RespectUser { get; set; }
        public int RideHorse { get; set; }
        public int SSOTicket { get; set; }
        public int SaveBotAction { get; set; }
        public int SaveBrandingItem { get; set; }
        public int SaveEnforcedCategorySettings { get; set; }
        public int SaveFloorPlanModel { get; set; }
        public int SaveRoomSettings { get; set; }
        public int SaveWardrobeOutfit { get; set; }
        public int SaveWiredConditionConfig { get; set; }
        public int SaveWiredEffectConfig { get; set; }
        public int SaveWiredTriggerConfig { get; set; }
        public int ScrGetUserInfo { get; set; }
        public int SendBullyReport { get; set; }
        public int SendMsg { get; set; }
        public int SendRoomInvite { get; set; }
        public int SetActivatedBadges { get; set; }
        public int SetChatPreference { get; set; }
        public int SetFriendBarState { get; set; }
        public int SetGroupFavourite { get; set; }
        public int SetMannequinFigure { get; set; }
        public int SetMannequinName { get; set; }
        public int SetMessengerInviteStatus { get; set; }
        public int SetRelationship { get; set; }
        public int SetSoundSettings { get; set; }
        public int SetToner { get; set; }
        public int SetUserFocusPreferenceEvent { get; set; }
        public int SetUsername { get; set; }
        public int Shout { get; set; }
        public int Sit { get; set; }
        public int StartQuest { get; set; }
        public int StartTyping { get; set; }
        public int SubmitBullyReport { get; set; }
        public int SubmitNewTicket { get; set; }
        public int TakeAdminRights { get; set; }
        public int ThrowDice { get; set; }
        public int ToggleMoodlight { get; set; }
        public int ToggleMuteTool { get; set; }
        public int ToggleYouTubeVideo { get; set; }
        public int TradingAccept { get; set; }
        public int TradingCancel { get; set; }
        public int TradingCancelConfirm { get; set; }
        public int TradingConfirm { get; set; }
        public int TradingModify { get; set; }
        public int TradingOfferItem { get; set; }
        public int TradingOfferItems { get; set; }
        public int TradingRemoveItem { get; set; }
        public int UnIgnoreUser { get; set; }
        public int UnbanUserFromRoom { get; set; }
        public int UniqueID { get; set; }
        public int UpdateFigureData { get; set; }
        public int UpdateForumSettings { get; set; }
        public int UpdateGroupBadge { get; set; }
        public int UpdateGroupColours { get; set; }
        public int UpdateGroupIdentity { get; set; }
        public int UpdateGroupSettings { get; set; }
        public int UpdateMagicTile { get; set; }
        public int UpdateNavigatorSettings { get; set; }
        public int UpdateStickyNote { get; set; }
        public int UpdateThread { get; set; }
        public int UseFurniture { get; set; }
        public int UseHabboWheel { get; set; }
        public int UseOneWayGate { get; set; }
        public int UseSellableClothing { get; set; }
        public int UseWallItem { get; set; }
        public int Whisper { get; set; }
        public int YouTubeGetNextVideo { get; set; }
        public int YouTubeVideoInformation { get; set; }
        #endregion

        static Outgoing()
        {
            _serializer = new DataContractJsonSerializer(typeof(Outgoing));

            Global = new Outgoing();
        }

        public void Save(string path)
        {
            using (var fileStream = File.Open(path, FileMode.Create))
                _serializer.WriteObject(fileStream, this);
        }
        public static Outgoing Load(string path)
        {
            using (var fileStream = File.Open(path, FileMode.Open))
                return (Outgoing)_serializer.ReadObject(fileStream);
        }
    }
}