namespace EMedicineBE.Models
{
    public class UploadProfilePicRequest
    {
        public IFormFile file { get; set; }
        public int user_id { get; set; }
    }
}
