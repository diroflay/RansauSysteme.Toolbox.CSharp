using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RansauSysteme.Utils
{
    /// <summary>
    /// Specifies the strength level required for password validation.
    /// </summary>
    public enum PasswordStrength
    {
        /// <summary>
        /// Requires only minimum length.
        /// </summary>
        Basic,

        /// <summary>
        /// Requires minimum length, uppercase and lowercase letters.
        /// </summary>
        Medium,

        /// <summary>
        /// Requires minimum length, uppercase, lowercase, and digits.
        /// </summary>
        Strong,

        /// <summary>
        /// Requires minimum length, uppercase, lowercase, digits, and special characters.
        /// </summary>
        VeryStrong
    }
}