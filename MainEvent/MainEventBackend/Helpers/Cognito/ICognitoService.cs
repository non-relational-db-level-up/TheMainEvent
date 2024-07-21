namespace MainEvent.Helpers.Cognito;

/// <summary>
/// Interface for Cognito Service
/// </summary>
public interface ICognitoService
{
    /// <summary>
    /// Gets the Cognito User
    /// </summary>
    /// <returns>The Cognito User</returns>
    CognitoUser Get();

    /// <summary>
    /// Sets the Cognito User
    /// </summary>
    /// <param name="cognitoUser">The Cognito User to set</param>
    void Set(CognitoUser cognitoUser);
}