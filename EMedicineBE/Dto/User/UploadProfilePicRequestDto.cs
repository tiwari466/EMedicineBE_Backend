namespace EMedicineBE.Dto.User
{
    public class UploadProfilePicRequestDto
    {
        public IFormFile file { get; set; }
        public int user_id { get; set; }
    }
}
