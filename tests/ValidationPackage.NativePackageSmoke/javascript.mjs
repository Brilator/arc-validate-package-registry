import {
  CommandInputBinding,
  CommandInputParameter,
  CommandInputType,
  CwlPrimitive,
  ValidationPackageMetadata
} from "@nfdi4plants/validationpackage-model";
import { ValidationPackageJson } from "@nfdi4plants/validationpackage-codecs";

const metadata = ValidationPackageMetadata.create(
  "native-package",
  "Native package smoke test",
  "Verifies the installed JavaScript dependency boundary.",
  1,
  2,
  3,
  "FSharp"
);
metadata.Inputs = [
  CommandInputParameter.create(
    "arc-directory",
    CommandInputType.create(CwlPrimitive.String),
    CommandInputBinding.create(undefined, "--arc-directory", false)
  )
];

const json = ValidationPackageJson.encode(metadata);
const decoded = ValidationPackageJson.decodeOrFail(json);

if (
  decoded.Name !== "native-package" ||
  decoded.MajorVersion !== 1 ||
  decoded.Inputs[0].InputBinding.Prefix !== "--arc-directory"
) {
  throw new Error("ValidationPackage JavaScript package round-trip failed");
}
