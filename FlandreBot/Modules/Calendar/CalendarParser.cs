using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Module.Calendar {
    public class CalendarParser {
        public static string DateFormat = "yyyyMMddTHHmmssZ";
        public static string DateFormatAlt = "yyyyMMdd";
        private string _buffer;
        private string _lookahead;
        private int _pointer;
        private bool _ignoreColon;
        private string _scan() {
            StringBuilder word = new StringBuilder();
            while(_buffer[_pointer] == ':' || _buffer[_pointer] == '\n')
                ++_pointer;

            while(_buffer[_pointer] != ':' || _ignoreColon) {
                if(_buffer[_pointer] == '\n') {
                    if(_buffer[_pointer+1] != ' ') {
                        _ignoreColon = false;
                        return word.ToString();
                    }
                    else
                        _pointer += 2;
                }
                word.Append(_buffer[_pointer++]);
            }
            _ignoreColon = true;
            return word.ToString();
        }
        private void _match(string token) {
            if(token == _lookahead) {
                _lookahead = _scan();
                return;
            }
            else {
                throw new InvalidDataException("[CalendarParser]: Error matching: " + token);
            }
        }
        private void _parseField(CalendarEvent calEvent) {
            switch(_lookahead) {
                case "DESCRIPTION":
                    _match("DESCRIPTION");
                    calEvent.Description = _lookahead;
                    _match(_lookahead);
                    break;
                case "DTSTART":
                    _match("DTSTART");
                    calEvent.DateStart = DateTime.ParseExact(_lookahead, DateFormat, CultureInfo.InvariantCulture);
                    _match(_lookahead);
                    break;
                case "DTSTART;VALUE=DATE":
                    _match("DTSTART;VALUE=DATE");
                    calEvent.DateStart = DateTime.ParseExact(_lookahead, DateFormatAlt, CultureInfo.InvariantCulture);
                    _match(_lookahead);
                    break;
                case "SUMMARY":
                    _match("SUMMARY");
                    calEvent.Summary = _lookahead;
                    _match(_lookahead);
                    break;
                default:
                    throw new InvalidDataException("[CalendarParser]: Error parsing FIELDs in VEVENT: " + _lookahead);
            }
            return;
        }
        private void _parseUseless() {
            while(_lookahead != "DESCRIPTION" && _lookahead != "DTSTART" && _lookahead != "DTSTART;VALUE=DATE"
                    && _lookahead != "SUMMARY" && _lookahead != "BEGIN" && _lookahead != "END")
                // Skip all the rubbish
                _match(_lookahead);
        }
        private CalendarEvent _parseEvent() {
            if(_lookahead == "BEGIN") {
                CalendarEvent currentEvent = new CalendarEvent();
                _match("BEGIN");
                _match("VEVENT");
                _parseUseless();
                _parseField(currentEvent);
                _parseUseless();
                _parseField(currentEvent);
                _parseUseless();
                _parseField(currentEvent);
                _parseUseless();
                _match("END");
                _match("VEVENT");
                return currentEvent;
            }
            else
                return null;
        }
        private void _parseEvents(List<CalendarEvent> eventList) {
            CalendarEvent currentEvent = _parseEvent();
            if(currentEvent == null)
                return;
            else
                eventList.Add(currentEvent);
            _parseEvents(eventList);
            return;
        }
        private List<CalendarEvent> _parseCalendar() {
            List<CalendarEvent> eventList = new List<CalendarEvent>();
            _lookahead = _scan();
            _match("BEGIN");
            _match("VCALENDAR");
            _parseUseless();
            _parseEvents(eventList);
            _match("END");
            _match("VCALENDAR");

            return eventList;
        }
        public async Task<List<CalendarEvent>> Parse(string input) {
            return await Task.Run(() => {
                _buffer = input.Replace("\r", "") + "placeholder:";
                return _parseCalendar();
            });
        }
    }
}
