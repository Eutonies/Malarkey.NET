using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions;
public static class MalarkeyConstants
{
    public const string MalarkeyAuthenticationScheme = "Malarkey.Session";

    public static class Authentication
    {
        public const string IdentityCookieBaseName = "Malarkey.Identity";
        public const string ProfileCookieName = "Malarkey.Profile";
        public const string TokenIssuer = "eutonies.com/malarkey";
        public const string TokenAlgorithm = "RS256";
        public const string TokenType = "JWT";
        public const string AudienceHeaderName = API.ClientCertificateHeaderName;

    }


    public static class API
    {
        public const string ClientCertificateHeaderName = "Malarkey.ClientCertificate";
    }


    public static class AuthenticationRequestQueryParameters
    {
        public const string ForwarderName = "forwarder";
        public const string ScopesName = "scopes";
        public const string IdProviderName = "idprovider";
    }


    public static class AuthenticationSuccessQueryParameters
    {
        /// <summary>
        /// If Malarkey authentication succeeds, profile token will returned as <see cref="ProfileTokenName"/> query parameter on forward
        /// </summary>
        public const string ProfileTokenName = "malarkeyprofiletoken";
        /// <summary>
        /// If Malarkey authentication succeeds, identity token for employed provider will returned as <see cref="IdentityTokenName"/> query parameter on forward
        /// </summary>
        public const string IdentityTokenName = "malarkeyidentitytoken";

        /// <summary>
        /// If the access-token obtained from identity provider is transferable for use by client API it will be returned as <see cref="IdentityProviderAccessTokenName"/> query parameter on forward
        /// </summary>
        public const string IdentityProviderAccessTokenName = "malarkeyidptoken";
    }

    


    public static class Scopes
    {
        public const string OpenIdConnect = "openid";


        public static class Spotify
        {
            /// <summary>
            /// Write access to user-provided images. 
            /// Upload images to Spotify on your behalf.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Add Custom Playlist Cover Image</item>
            /// </list>
            /// </summary>
            public const string ImageUpload = "ugc-image-upload";

            /// <summary>
            /// Read access to a user’s player state.
            /// Read your currently playing content and Spotify Connect devices information.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Get a User's Available Devices</item>
            ///   <item>Get Information About The User's Current Playback</item>
            ///   <item>Get the User's Currently Playing Track</item>
            /// </list>
            /// </summary>
            public const string UserReadPlaybackState = "user-read-playback-state";
            /// <summary>
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item> </item>
            /// </list>
            /// </summary>
            public const string UserModifyPlaybackState = "user-modify-playback-state";
            /// <summary>
            /// Write access to a user’s playback state.
            /// Control playback on your Spotify clients and Spotify Connect devices.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Pause a User's Playback</item>
            ///   <item>Seek To Position In Currently Playing Track</item>
            ///   <item>Set Repeat Mode On User’s Playback</item>
            ///   <item>Set Volume For User's Playback</item>
            ///   <item>Skip User’s Playback To Next Track</item>
            ///   <item>Skip User’s Playback To Previous Track</item>
            ///   <item>Start/Resume a User's Playback</item>
            ///   <item>Toggle Shuffle For User’s Playback</item>
            ///   <item>Transfer a User's Playback</item>
            ///   <item>Add An Item To The End Of User's Current Playback Queue</item>
            /// </list>
            /// </summary>
            public const string UserReadCurrentlyPlaying = "user-read-currently-playing";

            /// <summary>
            /// Read access to a user’s currently playing content.
            /// Read your currently playing content.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Get the User's Currently Playing Track</item>
            ///   <item>Get the User's Queue</item>
            /// </list>
            /// </summary>
            public const string AppRemoteControl = "app-remote-control";
            /// <summary>
            /// Remote control playback of Spotify. This scope is currently available to Spotify iOS and Android SDKs.
            /// Communicate with the Spotify app on your device.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>iOS SDK</item>
            ///   <item>Android SDK</item>
            /// </list>
            /// </summary>
            public const string Streaming = "streaming";

            /// <summary>
            /// Control playback of a Spotify track. This scope is currently available to the Web Playback SDK. The user must have a Spotify Premium account.
            /// Play content and control playback on your other devices.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Web Playback SDK</item>
            /// </list>
            /// </summary>
            public const string PlaylistReadPrivate = "playlist-read-private";
            /// <summary>
            /// Read access to user's private playlists.
            /// Access your private playlists.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Check if Users Follow a Playlist</item>
            ///   <item>Get a List of Current User's Playlists</item>
            ///   <item>Get a List of a User's Playlists</item>
            /// </list>
            /// </summary>
            public const string PlaylistReadCollaborative = "playlist-read-collaborative";
            /// <summary>
            /// Write access to a user's private playlists.
            /// Manage your private playlists.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Follow a Playlist</item>
            ///   <item>Unfollow a Playlist</item>
            ///   <item>Add Items to a Playlist</item>
            ///   <item>Change a Playlist's Details</item>
            ///   <item>Create a Playlist</item>
            ///   <item>Remove Items from a Playlist</item>
            ///   <item>Reorder a Playlist's Items</item>
            ///   <item>Replace a Playlist's Items</item>
            ///   <item>Upload a Custom Playlist Cover Image</item>
            /// </list>
            /// </summary>
            public const string PlaylistModifyPrivate = "playlist-modify-private";
            /// <summary>
            /// Write access to a user's public playlists.
            /// Manage your public playlists.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Follow a Playlist</item>
            ///   <item>Unfollow a Playlist</item>
            ///   <item>Add Items to a Playlist</item>
            ///   <item>Change a Playlist's Details</item>
            ///   <item>Create a Playlist</item>
            ///   <item>Remove Items from a Playlist</item>
            ///   <item>Reorder a Playlist's Items</item>
            ///   <item>Replace a Playlist's Items</item>
            ///   <item>Upload a Custom Playlist Cover Image</item>
            /// </list>
            /// </summary>
            public const string PlaylistModifyPublic = "playlist-modify-public";

            /// <summary>
            /// Write/delete access to the list of artists and other users that the user follows.
            /// Manage who you are following.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Follow Artists or Users</item>
            ///   <item>Unfollow Artists or Users</item>
            /// </list>
            /// </summary>
            public const string UserFollowModify = "user-follow-modify";
            /// <summary>
            /// Read access to the list of artists and other users that the user follows.
            /// Access your followers and who you are following.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Check if Current User Follows Artists or Users</item>
            ///   <item>Get User's Followed Artists</item>
            /// </list>
            /// </summary>
            public const string UserFollowRead = "user-follow-read";

            /// <summary>
            /// Read access to a user’s playback position in a content.
            /// Read your position in content you have played.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Get an Episodes</item>
            ///   <item>Get Several Episodes</item>
            ///   <item>Get a Show</item>
            ///   <item>Get Several Shows</item>
            ///   <item>Get a Show's Episodes</item>
            /// </list>
            /// </summary>
            public const string UserReadPlaybackPosition = "user-read-playback-position";

            /// <summary>
            /// Read access to a user's top artists and tracks.
            /// Read your top artists and content.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Get a User's Top Artists and Tracks</item>
            /// </list>
            /// </summary>
            public const string UserTopRead = "user-top-read";

            /// <summary>
            /// Read access to a user’s recently played tracks.
            /// Access your recently played items.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Get Current User's Recently Played Tracks</item>
            /// </list>
            /// </summary>
            public const string UserReadRecentlyPlayed = "user-read-recently-played";


            /// <summary>
            /// Write/delete access to a user's 'Your Music' library
            /// Manage your saved content.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Remove Albums for Current User</item>
            ///   <item>Remove User's Saved Tracks</item>
            ///   <item>Remove User's Saved Episodes</item>
            ///   <item>Save Albums for Current User</item>
            ///   <item>Save Tracks for User</item>
            ///   <item>Save Episodes for User</item>
            /// </list>
            /// </summary>
            public const string UserLibraryModify = "user-library-modify";
            /// <summary>
            /// Read access to a user's library.
            /// Access your saved content.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Check User's Saved Albums</item>
            ///   <item>Check User's Saved Tracks</item>
            ///   <item>Get Current User's Saved Albums</item>
            ///   <item>Get a User's Saved Tracks</item>
            ///   <item>Check User's Saved Episodes</item>
            ///   <item>Get User's Saved Episodes</item>
            /// </list>
            /// </summary>
            public const string UserLibraryRead = "user-library-read";

            /// <summary>
            /// Read access to user’s email address.
            /// Get your real email address.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Get Current User's Profile</item>
            /// </list>
            /// </summary>
            public const string UserReadEmail = "user-read-email";



            /// <summary>
            /// Read access to user’s subscription details (type of user account).
            /// Access your subscription details.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Search for an Item</item>
            ///   <item>Get Current User's Profile</item>
            /// </list>
            /// </summary>
            public const string UserReadPrivate = "user-read-private";

            /// <summary>
            /// Link a partner user account to a Spotify user account.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Register new user</item>
            /// </list>
            /// </summary>
            /// 
            public const string UserSoaLink = "user-soa-link";

            /// <summary>
            /// Unlink a partner user account from a Spotify account.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Unlink user</item>
            /// </list>
            /// </summary>
            public const string UserSoaUnlink = "user-soa-unlink";

            /// <summary>
            /// Modify entitlements for linked users.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Add user entitlements</item>
            ///   <item>Get user entitlements</item>
            ///   <item>Removes user entitlements</item>
            ///   <item>Replace user entitlements</item>
            /// </list>
            /// </summary>
            public const string SoaManageEntitlements = "soa-manage-entitlements";

            /// <summary>
            /// Update partner information.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Set partner logo</item>
            /// </list>
            /// </summary>
            public const string SoaManagePartner = "soa-manage-partner";
            /// <summary>
            /// Create new partners, platform partners only.
            /// 
            /// Needed for endpoints:
            /// <list type="bullet">
            ///   <item>Create new partner</item>
            /// </list>
            /// </summary>
            public const string SoaCreatePartner = "soa-create-partner";


        }

    }

}
