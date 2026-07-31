from validation_package_model import (
    CommandInputBinding,
    CommandInputParameter,
    CommandInputType,
    CwlPrimitive,
    ValidationPackageMetadata,
)
from validation_package_codecs import ValidationPackageJson

metadata = ValidationPackageMetadata.create(
    "native-package",
    "Native package smoke test",
    "Verifies the installed Python dependency boundary.",
    1,
    2,
    3,
    "FSharp",
)
metadata.Inputs = [
    CommandInputParameter.create(
        "arc-directory",
        CommandInputType.create(CwlPrimitive.String),
        CommandInputBinding.create(None, "--arc-directory", False),
    )
]

json = ValidationPackageJson.encode(metadata)
decoded = ValidationPackageJson.decode_or_fail(json)

assert decoded.Name == "native-package"
assert decoded.MajorVersion == 1
assert decoded.Inputs[0].InputBinding.Prefix == "--arc-directory"