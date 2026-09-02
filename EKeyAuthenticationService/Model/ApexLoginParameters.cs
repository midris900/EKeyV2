namespace EKeyAuthenticationService.Model
{
    public class ApexLoginParameters
    {
        /// <summary>
        /// it should be the first parameter in query string appended like f?p=139  (139 is the AppId)
        /// </summary>
        public int AppId { get; set; }

        /// <summary>
        /// it should be the second parameter in query string appended like f?p=139:9999  (9999 is the PageId)
        /// </summary>
        public int PageId { get; set; }

        /// <summary>
        /// it should be the third parameter in query string appended like f?p=139:9999:78978978  (78978978 is the AppSessionId)
        /// </summary>
        public string AppSessionId { get; set; } = string.Empty;

        /// <summary>
        /// Append above query string parameters to the base URL of the APEX application
        /// <remarks>
        /// The Url Should be like: https://apex.example.com/f?p=139:9999:78978978:::::auth,sessionId:authValue,sessionIdValue 
        /// </remarks>
        /// </summary>
        public string RedirectUrl { get; set; } = string.Empty;
    }
}
