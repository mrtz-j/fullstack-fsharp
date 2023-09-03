{ pkgs ? import <nixpkgs> {} }:

pkgs.mkShell {
  nativeBuildInputs = with pkgs; [
    nodejs-18_x
    dotnet-sdk_7
    nodePackages_latest.pnpm
  ];
}
