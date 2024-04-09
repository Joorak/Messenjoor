namespace Messenjoor.Shared.Models
{
    public record MessageModel(int ToUserId, int FromUserId, string Content, DateTime SentOn);
}
