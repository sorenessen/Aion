#!/bin/zsh

set -u

EST_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"

listener_pid() {
    local port="$1"
    lsof -tiTCP:"$port" -sTCP:LISTEN 2>/dev/null | head -1
}

process_command() {
    local pid="$1"
    ps -p "$pid" -o command= 2>/dev/null || true
}

process_cwd() {
    local pid="$1"
    lsof -a -p "$pid" -d cwd -Fn 2>/dev/null |
        sed -n 's/^n//p' |
        head -1
}

wait_for_url() {
    local url="$1"
    local attempts="${2:-40}"

    for ((i = 1; i <= attempts; i++)); do
        if curl -fsS --max-time 1 "$url" >/dev/null 2>&1; then
            return 0
        fi

        sleep 0.25
    done

    return 1
}

open_iterm_window() {
    local command="$1"
    local inherited_path="$PATH"

    local wrapped_command
    wrapped_command="/usr/bin/env PATH=${(q)inherited_path} /bin/zsh -f -c ${(q)command}"

    osascript - "$wrapped_command" <<'APPLESCRIPT'
on run argv
    set shellCommand to item 1 of argv

    tell application "iTerm"
        activate
        create window with default profile command shellCommand
    end tell
end run
APPLESCRIPT
}
