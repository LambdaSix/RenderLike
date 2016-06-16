namespace RenderLike
{
    // ReSharper disable once InconsistentNaming
    // Function names are lowerCamelCase so as not to clash with System.Char::

    public static class CharExtensions
    {
        public static bool isControl(this char self)
        {
            return (self >= 0x00 && self <= 0x08)       // Control codes
                   || (self == 0x09)                    // Tab
                   || (self >= 0x0A && self <= 0x0D)    // Whitespace
                   || (self >= 0x0E && self <= 0x1F);   // Extra control codes
        }

        /// <summary>
        /// Analogue to std::isprint()
        /// </summary>
        public static bool isPrintable(this char self)
        {
            return (self >= 0x20 && self <= 0x7E);
        }

        /// <summary>
        /// Analogue to std::isspace()
        /// </summary>
        public static bool isSpace(this char self)
        {
            switch (self)
            {
                case ' ':
                case '\n':
                case '\f':
                case '\r':
                case '\t':
                case '\v':
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Analogue to std::isblank()
        /// </summary>
        public static bool isBlank(this char self)
        {
            // horizontal tab and space
            return (self == 0x09 || self == 0x20);
        }

        /// <summary>
        /// Analogue to std::isgraph()
        /// </summary>        
        public static bool isGraph(this char self)
        {
            return self.isPrintable();
        }

        /// <summary>
        /// Analogue to std::ispunct()
        /// </summary>
        public static bool isPunctuation(this char self)
        {
            return (self >= 0x21 && self <= 0x2F)
                   || (self >= 0x3A && self <= 0x40)
                   || (self >= 0x5B && self <= 0x60)
                   || (self >= 0x7B && self <= 0x7E);
        }

        /// <summary>
        /// Analogue to std::isalnum()
        /// </summary>
        public static bool isAlphaNumeric(this char self)
        {
            return (self >= 0x30 && self <= 0x39) || self.isAlpha();
        }

        /// <summary>
        /// Analogue to std::isalpha()
        /// </summary>
        public static bool isAlpha(this char self)
        {
            return self.isUpper() || self.isLower();
        }

        /// <summary>
        /// Analogue to std::isupper()
        /// </summary>
        public static bool isUpper(this char self)
        {
            return (self >= 0x41 && self <= 0x5A);
        }

        /// <summary>
        /// Analogue to std::islower()
        /// </summary>       
        public static bool isLower(this char self)
        {
            return (self >= 0x61 && self <= 0x7A);
        }

        /// <summary>
        /// Analogue to std::isdigit()
        /// </summary>        
        public static bool isDigit(this char self)
        {
            return (self >= 0x30 && self <= 0x39);
        }

        /// <summary>
        /// Analogue to std::isxdigit()
        /// </summary>
        public static bool isXDigit(this char self)
        {
            return self.isDigit()
                   || (self >= 0x61 && self <= 0x66)
                   || (self >= 0x41 && self <= 0x46);
        }
    }
}