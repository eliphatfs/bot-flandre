using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Module.Calendar {
    public class CalendarParser {
        public static string DateFormat = "yyyyMMddTHHmmssZ";
        public static string DateFormatAlt = "yyyyMMdd";
        private string _buffer;
        private int _line;
        private string _lookahead;
        private int _pointer;
        private bool _ignoreColon;
        private string _scan() {
            StringBuilder word = new StringBuilder();
            while(_buffer[_pointer] == ':' || _buffer[_pointer] == '\n') {
                if(_buffer[_pointer] == '\n')
                    ++_line;
                ++_pointer;
            }
                

            while(_buffer[_pointer] != ':' || _ignoreColon) {
                if(_buffer[_pointer] == '\n') {
                    ++_line;
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
                throw new InvalidDataException("[CalendarParser]: Error matching at line " + _line.ToString() + "\n Expected: " + token + "; Got: " + _lookahead);
            }
        }
        private string _stringProcessingPipeline() {
            _lookahead = _lookahead.Replace("\\,",",").Replace("\\n", "\n");
            Regex courseName = new Regex(@"本.*[)][]-]"); // remove redundant info in course name
            Regex courseNumber = new Regex(@"-.*-");
            Regex mdHyperRef = new Regex(@"[]]\s+[(]"); // remove space between markdown hrefs

            _lookahead = courseName.Replace(_lookahead, "");
            _lookahead = courseNumber.Replace(_lookahead, "-");
            _lookahead = mdHyperRef.Replace(_lookahead, "](");

            return _lookahead;
        }
        private bool _parseField(CalendarEvent currentEvent) {
            if(_lookahead == "END")
                return false;
            else {
                string fieldKey = _lookahead;
                _match(_lookahead);
                switch(fieldKey) {
                    case "DESCRIPTION":
                        currentEvent.Description = _stringProcessingPipeline();
                        break;
                    case "DTSTART;VALUE=DATE":
                        currentEvent.DateStart = DateTime.ParseExact(_lookahead, DateFormatAlt, CultureInfo.InvariantCulture);
                        break;
                    case "DTSTART":
                        currentEvent.DateStart = DateTime.ParseExact(_lookahead, DateFormat, CultureInfo.InvariantCulture);
                        break;
                    case "SUMMARY":
                        currentEvent.Summary = _stringProcessingPipeline();
                        break;
                }
                _match(_lookahead);
            }
            return true;
        }
        private void _parseFields(CalendarEvent currentEvent) {
            if(_parseField(currentEvent)) {
                _parseFields(currentEvent);
            }
        }
        private CalendarEvent _parseEvent() {
            if(_lookahead == "BEGIN") {
                CalendarEvent currentEvent = new CalendarEvent();
                _match("BEGIN");
                _match("VEVENT");
                _parseFields(currentEvent);
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
            while(_lookahead != "BEGIN") {
                _match(_lookahead);
            }
            _parseEvents(eventList);
            _match("END");
            _match("VCALENDAR");

            return eventList;
        }
        public async Task<List<CalendarEvent>> Parse(string input) {
            _line = 1;
            return await Task.Run(() => {
                _buffer = input.Replace("\r", "") + "placeholder:";
                return _parseCalendar();
            });
        }
    }
}
