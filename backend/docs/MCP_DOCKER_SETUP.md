# MCP Docker Setup for context7

This guide provides the correct configuration for running the `mcp/context7` Docker image as a stdio-based MCP server for use with clients like Cursor.

## The Problem

The `mcp/context7` Docker image defaults to running in **HTTP mode**, which is incompatible with clients that expect a **stdio** (standard input/output) based server. The standard instructions often provided are incomplete and will not work without modification.

## The Fix: Correct `mcp.json` Configuration

To get the `mcp/context7` server working correctly, you need to use the following configuration in your `mcp.json` file. This configuration explicitly sets the server to run in `stdio` mode.

```json
{
  "mcpServers": {
    "context7": {
      "command": "docker",
      "args": [
        "run",
        "-i",
        "--rm",
        "-e",
        "MCP_TRANSPORT=stdio",
        "--entrypoint=",
        "mcp/context7",
        "node",
        "dist/index.js"
      ],
      "env": {
        "MCP_TRANSPORT": "stdio"
      }
    }
  }
}
```

### Breakdown of the Configuration:

- `"command": "docker"`: Specifies that we are using Docker to run the server.
- `"args": [...]`: This is the crucial part.
    - `"run", "-i", "--rm"`: Standard Docker flags to run a container interactively and remove it after it exits.
    - `"-e", "MCP_TRANSPORT=stdio"`: This environment variable tells the `context7` server to run in **stdio mode**, which is required for Cursor.
    - `"--entrypoint="`: This clears the default entrypoint of the Docker image, which might be configured for HTTP mode.
    - `"mcp/context7"`: The name of the Docker image.
    - `"node", "dist/index.js"`: This is the command that directly runs the MCP server script inside the container.
- `"env": { "MCP_TRANSPORT": "stdio" }`: This also ensures the environment variable is set for the process.

## Next Steps on a New Machine

1.  Create the `mcp.json` file in the appropriate directory for your client (e.g., `~/.cursor/mcp.json` for Cursor).
2.  Paste the configuration from this document into the file.
3.  Restart your client (e.g., Cursor).
4.  The `context7` MCP tools should now be available. 