"""Portable metadata and CWL input model for ARC validation packages."""

from enum import IntEnum

from .author import Author
from .cwl import CommandInputBinding, CommandInputParameter, CommandInputType
from .ontology_annotation import OntologyAnnotation
from .semantic_version import SemVer
from .validation_package_identity import ValidationPackageIdentity
from .validation_package_metadata import ValidationPackageMetadata


class CwlPrimitive(IntEnum):
    Boolean = 0
    Int = 1
    Long = 2
    Float = 3
    Double = 4
    String = 5

__all__ = [
    "Author",
    "CommandInputBinding",
    "CommandInputParameter",
    "CommandInputType",
    "CwlPrimitive",
    "OntologyAnnotation",
    "SemVer",
    "ValidationPackageIdentity",
    "ValidationPackageMetadata",
]