
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ivNet.Club.Models;
using Orchard;
using Orchard.Security;

namespace ivNet.Club.Services
{
    public interface IImageServices : IDependency
    {
        List<ClubImage> GetCarouselImages();
    }

    public class ImageServices : BaseService, IImageServices
    {
        public ImageServices(IAuthenticationService authenticationService) 
            : base(authenticationService)
        {
        }

        public List<ClubImage> GetCarouselImages()
        {
            var imageList = new List<ClubImage>();
            var root = HttpContext.Current.Server.MapPath("/");
            const string webRoot = "/Media/Default/Carousel";

            var carousel = string.Format("{0}{1}", root, "Media\\Default\\Carousel");
            if (!Directory.Exists(carousel)) return imageList;
            
            var files = Directory.GetFiles(carousel);
            imageList.AddRange(files.Select(file => new ClubImage
            {
                Url = string.Format("{0}/{1}",
                    webRoot,
                    file.Substring(file.LastIndexOf("\\", System.StringComparison.Ordinal) + 1))
            }));
            return imageList;
        }
    }
}