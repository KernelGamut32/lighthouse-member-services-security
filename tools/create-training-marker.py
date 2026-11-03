#!/usr/bin/env python3
"""Create a harmless local fixture for a repository-scoped custom secret pattern.

No credentials, network, commits, or pushes are involved. The prefix denotes a
workshop marker, not an Azure or other provider-issued credential.
"""

import argparse
from pathlib import Path
import secrets
import string


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path, required=True, help="New fixture file to create")
    parser.add_argument("--purpose", choices=("history", "push"), required=True)
    args = parser.parse_args()

    if args.output.exists():
        parser.error("Output already exists. Choose a new path; existing content is never overwritten.")

    alphabet = string.ascii_uppercase + string.digits
    marker = "LHTRAINING_" + "".join(secrets.choice(alphabet) for _ in range(32))
    content = (
        "HARMLESS WORKSHOP TRAINING MARKER. This is not a credential.\n"
        f"Purpose: {args.purpose}\n"
        "Detection, if any, uses an administrator-defined CUSTOM pattern.\n"
        f"training_marker={marker}\n"
    )
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with args.output.open("x", encoding="utf-8") as stream:
        stream.write(content)
    print(f"Created local {args.purpose} fixture: {args.output}")
    print("Custom pattern secret format: LHTRAINING_[A-Z0-9]{32}")
    print("No commit or push was performed. Review the file before using the facilitator runbook.")


if __name__ == "__main__":
    main()
