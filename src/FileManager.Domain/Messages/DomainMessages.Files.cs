namespace FileManager.Domain.Messages;

public static partial class DomainMessages
{
    public const string InvalidUploadLimitRange = "domain.invalid_upload_limit_range";
    public const string ApplicationNameExists = "domain.application_name_exists";
    public const string ApplicationNotFound = "domain.application_not_found";
    public const string ApplicationInactive = "domain.application_inactive";
    public const string InvalidToken = "domain.invalid_token";
    public const string FolderNotFound = "domain.folder_not_found";
    public const string FolderNotEmpty = "domain.folder_not_empty";
    public const string FolderNameExists = "domain.folder_name_exists";
    public const string FileNotFound = "domain.file_not_found";
    public const string FileNameExists = "domain.file_name_exists";
    public const string FileAlreadyDeleted = "domain.file_already_deleted";
    public const string FileNotInTrash = "domain.file_not_in_trash";
    public const string FileSizeOutOfRange = "domain.file_size_out_of_range";
    public const string UnsupportedFileType = "domain.unsupported_file_type";
    public const string TrashItemNotFound = "domain.trash_item_not_found";
    public const string TenantMismatch = "domain.tenant_mismatch";
    public const string InvalidParentFolder = "domain.invalid_parent_folder";
    public const string CircularFolderReference = "domain.circular_folder_reference";
    public const string RootFolderProtected = "domain.root_folder_protected";
}
