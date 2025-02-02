using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proofpoint.SecureEmailRelay.Mail
{
    public interface IMimeTypeMapper
    {
        public string GetMimeType(string fileName);
        public bool IsValidMimeType(string mimeType);
    }
}
