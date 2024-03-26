{
  description = "Fullstack F#";

  inputs = {
    flake-schemas.url = "https://flakehub.com/f/DeterminateSystems/flake-schemas/*.tar.gz";

    nixpkgs.url = "https://flakehub.com/f/NixOS/nixpkgs/0.1.*.tar.gz";
  };

  outputs = {
    self,
    flake-schemas,
    nixpkgs,
  }: let
    supportedSystems = ["x86_64-linunx" "aarch64-darwin"];
    forEachSupportedSystem = f:
      nixpkgs.lib.genAttrs supportedSystems (system:
        f {
          pkgs = import nixpkgs {inherit system;};
        });
  in {
    schemas = flake-schemas.schemas;

    devShells = forEachSupportedSystem ({pkgs}: {
      default = pkgs.mkShell {
        packages = with pkgs; [
          nixpkgs-fmt
          fsautocomplete
          dotnet-sdk_8
          bun
          nodejs
        ];
      };
    });
  };
}
