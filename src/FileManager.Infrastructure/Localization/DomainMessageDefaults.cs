using FileManager.Domain.Messages;
using Kootam.Translator.Database.Options;

namespace FileManager.Infrastructure.Localization;

internal static class DomainMessageDefaults
{
    public static DefaultTranslationOption[] Create() =>
    [
        ..CreateCulture("en-US", English),
        ..CreateCulture("fa-IR", Persian)
    ];

    private static IEnumerable<DefaultTranslationOption> CreateCulture(
        string culture,
        IReadOnlyDictionary<string, string> messages)
    {
        foreach (var (key, value) in messages)
        {
            yield return new DefaultTranslationOption
            {
                Key = key,
                Value = value,
                Culture = culture
            };
        }
    }

    private static readonly IReadOnlyDictionary<string, string> English =
        new Dictionary<string, string>
        {
            [DomainMessages.Required] = "{0} is required.",
            [DomainMessages.MinLength] = "{0} must be at least {1} characters.",
            [DomainMessages.MaxLength] = "{0} must not exceed {1} characters.",
            [DomainMessages.Positive] = "{0} must be positive.",
            [DomainMessages.NotNegative] = "{0} must not be negative.",
            [DomainMessages.InvalidUploadLimitRange] = "Invalid upload limit range for {0}.",
            [DomainMessages.ApplicationNameExists] = "An application with this name already exists.",
            [DomainMessages.ApplicationNotFound] = "Application not found.",
            [DomainMessages.ApplicationInactive] = "Application is inactive.",
            [DomainMessages.InvalidToken] = "Invalid token.",
            [DomainMessages.FolderNotFound] = "Folder not found.",
            [DomainMessages.FolderNotEmpty] = "Folder is not empty.",
            [DomainMessages.FolderNameExists] = "A folder with this name already exists.",
            [DomainMessages.FileNotFound] = "File not found.",
            [DomainMessages.FileNameExists] = "A file with this name already exists.",
            [DomainMessages.FileAlreadyDeleted] = "File is already deleted.",
            [DomainMessages.FileNotInTrash] = "File is not in trash.",
            [DomainMessages.FileSizeOutOfRange] = "File size is out of the allowed range.",
            [DomainMessages.UnsupportedFileType] = "Unsupported file type.",
            [DomainMessages.TrashItemNotFound] = "Trash item not found.",
            [DomainMessages.TenantMismatch] = "Tenant mismatch.",
            [DomainMessages.InvalidParentFolder] = "Invalid parent folder.",
            [DomainMessages.CircularFolderReference] = "Circular folder reference is not allowed.",
            [DomainMessages.RootFolderProtected] = "The root folder cannot be modified.",
            [DomainMessages.ApplicationNameRequired] = "Application name is required.",
            [DomainMessages.TokenRequired] = "Token is required.",
            [DomainMessages.TrashItemAlreadyPurged] = "Trash item has already been purged.",
            [DomainMessages.TrashItemAlreadyRestored] = "Trash item has already been restored.",
            [DomainMessages.ItemAlreadyInTrash] = "Item is already in trash."
        };

    private static readonly IReadOnlyDictionary<string, string> Persian =
        new Dictionary<string, string>
        {
            [DomainMessages.Required] = "{0} الزامی است.",
            [DomainMessages.MinLength] = "{0} باید حداقل {1} کاراکتر باشد.",
            [DomainMessages.MaxLength] = "{0} نباید بیشتر از {1} کاراکتر باشد.",
            [DomainMessages.Positive] = "{0} باید مثبت باشد.",
            [DomainMessages.NotNegative] = "{0} نمی‌تواند منفی باشد.",
            [DomainMessages.InvalidUploadLimitRange] = "محدوده نامعتبر برای {0}.",
            [DomainMessages.ApplicationNameExists] = "برنامه‌ای با این نام از قبل وجود دارد.",
            [DomainMessages.ApplicationNotFound] = "برنامه یافت نشد.",
            [DomainMessages.ApplicationInactive] = "برنامه غیرفعال است.",
            [DomainMessages.InvalidToken] = "توکن نامعتبر است.",
            [DomainMessages.FolderNotFound] = "پوشه یافت نشد.",
            [DomainMessages.FolderNotEmpty] = "پوشه خالی نیست.",
            [DomainMessages.FolderNameExists] = "پوشه‌ای با این نام از قبل وجود دارد.",
            [DomainMessages.FileNotFound] = "فایل یافت نشد.",
            [DomainMessages.FileNameExists] = "فایلی با این نام از قبل وجود دارد.",
            [DomainMessages.FileAlreadyDeleted] = "فایل قبلاً حذف شده است.",
            [DomainMessages.FileNotInTrash] = "فایل در سطل زباله نیست.",
            [DomainMessages.FileSizeOutOfRange] = "حجم فایل خارج از محدوده مجاز است.",
            [DomainMessages.UnsupportedFileType] = "نوع فایل پشتیبانی نمی‌شود.",
            [DomainMessages.TrashItemNotFound] = "آیتم سطل زباله یافت نشد.",
            [DomainMessages.TenantMismatch] = "عدم تطابق tenant.",
            [DomainMessages.InvalidParentFolder] = "پوشه والد نامعتبر است.",
            [DomainMessages.CircularFolderReference] = "ارجاع حلقوی بین پوشه‌ها مجاز نیست.",
            [DomainMessages.RootFolderProtected] = "پوشه ریشه قابل تغییر نیست.",
            [DomainMessages.ApplicationNameRequired] = "نام برنامه الزامی است.",
            [DomainMessages.TokenRequired] = "توکن الزامی است.",
            [DomainMessages.TrashItemAlreadyPurged] = "آیتم سطل زباله قبلاً حذف دائمی شده است.",
            [DomainMessages.TrashItemAlreadyRestored] = "آیتم سطل زباله قبلاً بازیابی شده است.",
            [DomainMessages.ItemAlreadyInTrash] = "آیتم از قبل در سطل زباله است."
        };
}
