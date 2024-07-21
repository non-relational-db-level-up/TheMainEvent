namespace MainEvent.Helpers.Cognito;

/// <summary>
/// A C# class implementing ICognitoService interface for managing Cognito users.
/// </summary>
public class CognitoService : ICognitoService
{
    /// <summary>
    /// Property for storing Cognito User.
    /// </summary>
    private CognitoUser CognitoUser { get; set; } = null!;

    /// <summary>
    /// Method to get the Cognito User.
    /// </summary>
    /// <returns>Returns the Cognito User.</returns>
    public CognitoUser Get()
    {
        return CognitoUser;
    }

    /// <summary>
    /// Method to set the Cognito User.
    /// </summary>
    /// <param name="cognitoUser">The Cognito User to set.</param>
    public void Set(CognitoUser cognitoUser)
    {
        CognitoUser = cognitoUser;
    }
}